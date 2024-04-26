using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Npgsql;
using Newtonsoft.Json;

class Program
{
    static async Task Main(string[] args)
    {
        // URL da API Random User Generator
        string apiUrl = "https://randomuser.me/api/?results=5";

        // Lista para armazenar os dados dos usuários
        List<User> users = new List<User>();

        // Criação de um objeto HttpClient
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Faz uma solicitação GET à API
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                // Verifica se a solicitação foi bem-sucedida
                if (response.IsSuccessStatusCode)
                {
                    // Lê o conteúdo da resposta como uma string
                    string json = await response.Content.ReadAsStringAsync();

                    // Converte o JSON para um objeto UserResult
                    UserResult result = JsonConvert.DeserializeObject<UserResult>(json);

                    // Verifica se foram retornados resultados
                    if (result != null && result.results.Length > 0)
                    {
                        // Adiciona os usuários à lista
                        users.AddRange(result.results);

                        // String de conexão com o PostgreSQL
                        string connectionString = "Host=localhost;Port=5432;Database=random_api_bd;Username=postgres;Password=Bd123@;";

                        // Conectar ao banco de dados
                        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                        {
                            await connection.OpenAsync();

                            // Iterar sobre os usuários e inseri-los na tabela
                            foreach (User user in users)
                            {
                          
                                string name = $"{user.name.first} {user.name.last}";

                                string sql = "INSERT INTO users (name, age, email, country) VALUES (@Name, @Age, @Email, @Country)";
                                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                                {
                                    command.Parameters.AddWithValue("@Name", name);
                                    command.Parameters.AddWithValue("@Age", user.dob.age);
                                    command.Parameters.AddWithValue("@Email", user.email);
                                    command.Parameters.AddWithValue("@Country", user.location.country);

                                    await command.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        Console.WriteLine("Dados inseridos com sucesso no banco de dados.");
                    }
                    else
                    {
                        Console.WriteLine("Nenhum usuário retornado.");
                    }
                }
                else
                {
                    Console.WriteLine("Falha na solicitação HTTP.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
            }
        }
    }
}

// Classes para converter os JSON
public class UserResult
{
    public User[] results { get; set; }
}

public class User
{
    public Name name { get; set; }
    public Dob dob { get; set; }
    public string email { get; set; }
    public Location location { get; set; }
    
}

public class Name
{
    public string first { get; set; }
    public string last { get; set; }

}

public class Dob
{
    public int age { get; set; }
}

public class Location
{
    public string country { get; set; }
   
}
