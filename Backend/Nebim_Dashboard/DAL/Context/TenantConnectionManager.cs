using System.Data;
using System.Security.Cryptography;
using System.Text;
using Entity.App;
using Entity.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DAL.Context;

/// <summary>
/// Tenant'ların Nebim bağlantılarını yöneten manager.
/// Connection string encryption/decryption, connection pooling ve test işlemleri.
/// </summary>
public interface ITenantConnectionManager
{
    /// <summary>
    /// Tenant'ın Nebim connection'ını alır.
    /// </summary>
    Task<IDbConnection> GetNebimConnectionAsync(int tenantId, CancellationToken ct = default);

    /// <summary>
    /// Tenant'ın Nebim bağlantısını test eder.
    /// </summary>
    Task<ConnectionTestResult> TestConnectionAsync(int tenantId, CancellationToken ct = default);

    /// <summary>
    /// Yeni connection string'i test eder (kaydetmeden).
    /// </summary>
    Task<ConnectionTestResult> TestConnectionStringAsync(string connectionString, CancellationToken ct = default);

    /// <summary>
    /// Connection string'i şifreler.
    /// </summary>
    string EncryptConnectionString(string connectionString);

    /// <summary>
    /// Connection string'i çözer.
    /// </summary>
    string DecryptConnectionString(string encryptedConnectionString);

    /// <summary>
    /// Tenant simulation modunda mı?
    /// </summary>
    Task<bool> IsSimulationModeAsync(int tenantId, CancellationToken ct = default);
}

/// <summary>
/// Bağlantı test sonucu.
/// </summary>
public class ConnectionTestResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ServerVersion { get; set; }
    public string? DatabaseName { get; set; }
    public long ResponseTimeMs { get; set; }

    public static ConnectionTestResult Success(string serverVersion, string databaseName, long responseTimeMs) => new()
    {
        IsSuccess = true,
        ServerVersion = serverVersion,
        DatabaseName = databaseName,
        ResponseTimeMs = responseTimeMs
    };

    public static ConnectionTestResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// ITenantConnectionManager implementasyonu.
/// </summary>
public class TenantConnectionManager : ITenantConnectionManager
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantConnectionManager> _logger;
    private readonly byte[] _encryptionKey;

    private const int ConnectionTimeoutSeconds = 10;

    public TenantConnectionManager(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<TenantConnectionManager> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;

        // Master encryption key from configuration
        var keyString = _configuration["Encryption:MasterKey"] 
            ?? throw new InvalidOperationException("Encryption:MasterKey is not configured");
        _encryptionKey = Convert.FromBase64String(keyString);
    }

    public async Task<IDbConnection> GetNebimConnectionAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct)
            ?? throw new InvalidOperationException($"Tenant not found: {tenantId}");

        if (tenant.NebimServerType == NebimServerType.Simulation)
        {
            throw new InvalidOperationException("Tenant is in simulation mode, cannot get real connection");
        }

        if (string.IsNullOrEmpty(tenant.NebimConnectionStringEncrypted))
        {
            throw new InvalidOperationException("Tenant has no Nebim connection configured");
        }

        var connectionString = DecryptConnectionString(tenant.NebimConnectionStringEncrypted);
        
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        
        return connection;
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

        if (tenant == null)
        {
            return ConnectionTestResult.Failure("Tenant not found");
        }

        if (string.IsNullOrEmpty(tenant.NebimConnectionStringEncrypted))
        {
            return ConnectionTestResult.Failure("No connection string configured");
        }

        var connectionString = DecryptConnectionString(tenant.NebimConnectionStringEncrypted);
        return await TestConnectionStringAsync(connectionString, ct);
    }

    public async Task<ConnectionTestResult> TestConnectionStringAsync(string connectionString, CancellationToken ct = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Add timeout to connection string if not present
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                ConnectTimeout = ConnectionTimeoutSeconds
            };

            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(ct);

            stopwatch.Stop();

            _logger.LogInformation(
                "Nebim connection test successful. Server: {Server}, Database: {Database}, Time: {Time}ms",
                connection.DataSource, connection.Database, stopwatch.ElapsedMilliseconds);

            return ConnectionTestResult.Success(
                connection.ServerVersion,
                connection.Database,
                stopwatch.ElapsedMilliseconds);
        }
        catch (SqlException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Nebim connection test failed: {Message}", ex.Message);
            return ConnectionTestResult.Failure($"SQL Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Nebim connection test failed: {Message}", ex.Message);
            return ConnectionTestResult.Failure($"Connection Error: {ex.Message}");
        }
    }

    public async Task<bool> IsSimulationModeAsync(int tenantId, CancellationToken ct = default)
    {
        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

        return tenant?.NebimServerType == NebimServerType.Simulation;
    }

    public string EncryptConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(connectionString);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Combine IV + encrypted data
        var result = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string DecryptConnectionString(string encryptedConnectionString)
    {
        if (string.IsNullOrEmpty(encryptedConnectionString))
            throw new ArgumentNullException(nameof(encryptedConnectionString));

        var fullCipher = Convert.FromBase64String(encryptedConnectionString);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        // Extract IV
        var iv = new byte[aes.IV.Length];
        var cipher = new byte[fullCipher.Length - iv.Length];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
