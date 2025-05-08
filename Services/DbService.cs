using APBD_CW_7.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace APBD_CW_7_W.Services;

public interface IDbService
{
    Task<IEnumerable<TripGetDTO>> GetAllTripsWithCountryAsync();
    Task<IEnumerable<ClientTripGetDTO>> GetAllClientsTrips(int clientId);
    Task<int?> AddClientAsync(ClientCreateDTO client);
    Task<String?> RegisterClientForTripAsync(int clientId, int tripId);
    Task<String?> DeleteClientAsync(int clientId,int tripId);

}

public class DbService(IConfiguration config) : IDbService
{
    // Metoda do wybrania wszystkich wycieczek jak i listy krajów w jakich jest organizowana wycieczka
    public async Task<IEnumerable<TripGetDTO>> GetAllTripsWithCountryAsync()
    {
        var result = new List<TripGetDTO>();

        var connectionString = config.GetConnectionString("Default");

        var sql = """
                  SELECT
                      T.IdTrip,
                      T.Name,
                      T.Description,
                      T.DateFrom,
                      T.DateTo,
                      T.MaxPeople,
                      STRING_AGG(C.Name, ', ') AS CountryList
                  FROM TRIP T
                           JOIN Country_Trip CT ON T.IdTrip = CT.IdTrip
                           JOIN Country C ON CT.IdCountry = C.IdCountry
                  GROUP BY T.IdTrip, T.Name, T.Description, T.DateFrom, T.DateTo, T.MaxPeople;

                  """;

        await using (var connection = new SqlConnection(connectionString))
        await using (var command = new SqlCommand(sql, connection))
        {
            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new TripGetDTO
                {
                    IdTrip = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    Countries = reader.GetString(6)
                });
            }
        }

        return result;
    }

    // Metoda do wybrania wszystkich wycieczek danego klienta
    public async Task<IEnumerable<ClientTripGetDTO>> GetAllClientsTrips(int clientId)
    {
        var result = new List<ClientTripGetDTO>();

        var connectionString = config.GetConnectionString("Default");
        var sql = """
                  SELECT Trip.IdTrip,
                         Name,
                         Description,
                         DateFrom,
                         DateTo,
                         MaxPeople,
                         RegisteredAt,
                         PaymentDate
                  FROM Trip
                           JOIN Client_Trip ON Trip.IdTrip = Client_Trip.IdTrip
                  WHERE Client_Trip.IdClient = @clientId
                  """;
        await using (var connection = new SqlConnection(connectionString))
        await using (var command = new SqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@clientId", clientId);

            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new ClientTripGetDTO
                {
                    IdTrip = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    MaxPeople = reader.GetInt32(5),
                    RegisteredAt = reader.GetInt32(6),
                    PaymentDate = reader.GetInt32(7)
                });
            }
        }

        return result;
    }

    // Dodaje nowego klienta
    public async Task<int?> AddClientAsync(ClientCreateDTO client)
    {
        var connectionString = config.GetConnectionString("Default");

        var sql = """
                  INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                  VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
                  SELECT CAST(SCOPE_IDENTITY() AS int);
                  """;

        await using (var connection = new SqlConnection(connectionString))
        await using (var command = new SqlCommand(sql, connection))
        {

            command.Parameters.AddWithValue("@FirstName", client.FirstName);
            command.Parameters.AddWithValue("@LastName", client.LastName);
            command.Parameters.AddWithValue("@Email", client.Email);
            command.Parameters.AddWithValue("@Telephone", client.Telephone);
            command.Parameters.AddWithValue("@Pesel", client.Pesel);

            await connection.OpenAsync();


            var result = await command.ExecuteScalarAsync();

            return result != null ? (int?)Convert.ToInt32(result) : null;
        }
    }
    // Dodaje nowy wpis do tabeli asocjacyjnej Client_Trip
    public async Task<String?> RegisterClientForTripAsync(int clientId, int tripId)
    {
        var connectionString = config.GetConnectionString("Default");

        var sqlCheckClient =
            """
              Select 1 From Client Where idClient = @clientId
            """;
        var sqlChecktrip =
            """
              Select MaxPeople From Trip Where idTrip = @tripId
            """;
        var sqlCountPeopleOnTrip =
            """
              Select Count(*) From Client_Trip Where idTrip = @tripId
            """;
        var sqlInsertValue = 
            """
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
            VALUES (@clientId, @tripId, CONVERT(INT, GETDATE()))
            """;


        await using (var connection = new SqlConnection(connectionString))
        await using (var command = new SqlCommand())
        {
            command.Connection = connection;

            await connection.OpenAsync();

            command.CommandText = sqlCheckClient;
            command.Parameters.AddWithValue("@clientId", clientId);
            var clientExists = await command.ExecuteScalarAsync() != null;

            if (!clientExists)
                return "Client not found";

            command.Parameters.Clear();

            command.CommandText = sqlChecktrip;
            command.Parameters.AddWithValue("@tripId", tripId);
            var maxPeopleObj = await command.ExecuteScalarAsync();

            if (maxPeopleObj == null)
                return "Trip not found";

            int maxPeople = (int)maxPeopleObj;
            command.CommandText = sqlCountPeopleOnTrip;

            int registeredPeople = (int)await command.ExecuteScalarAsync();

            if (maxPeople <= registeredPeople)
            {
                return "Trip is full";
            }

            command.Parameters.Clear();
            command.CommandText = sqlInsertValue;
            command.Parameters.AddWithValue("@clientId", clientId);
            command.Parameters.AddWithValue("tripId", tripId);

            int rows = await command.ExecuteNonQueryAsync();

            return rows > 0 ? null : "Error during registration";
        }
    }
    // Usuwa wpis z tabeli asocjacyjnej Client_Trip
    public async Task<String?> DeleteClientAsync(int clientId,int tripId)
    {
        var connectionString = config.GetConnectionString("Default");

        var sqlCheckForRegistration = """
                  Select 1 From Client_Trip
                  WHERE IdTrip = @tripId AND IdClient = @clientId 
                  """;
        var sqlDelete =
            """
            Delete from Client_Trip
            WHERE IdTrip = @tripId AND IdClient = @clientId 
            """;

        await using (var connection = new SqlConnection(connectionString))
        await using (var command = new SqlCommand(sqlCheckForRegistration, connection))
        {
            await connection.OpenAsync();
            command.Parameters.AddWithValue("@clientId", clientId);
            command.Parameters.AddWithValue("@tripId", tripId);

            var registrationExists = await command.ExecuteScalarAsync();
            if (registrationExists == null)
            {
                return "Registration does not exist";
            }
            
            command.Parameters.Clear();
            command.CommandText = sqlDelete;
            command.Parameters.AddWithValue("@clientId", clientId);
            command.Parameters.AddWithValue("@tripid", tripId);

            var rows = await command.ExecuteNonQueryAsync();
            return rows > 0 ? null : "Error while deleting registration";
        }
    }

}