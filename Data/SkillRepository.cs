using Microsoft.Data.Sqlite;
using RaidersVault.Models;

namespace RaidersVault.Data;

public class SkillRepository
{
    private readonly string _connectionString;

    public SkillRepository(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<List<Skill>> GetSkillsAsync()
    {
        var skills = new List<Skill>();

        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                s.Id,
                s.Branch,
                s.Name,
                s.Description,
                s.Details,
                s.MaxPoints,
                s.Requires,
                COALESCE(us.CurrentPoints, 0) AS CurrentPoints
            FROM Skills s
            LEFT JOIN UserSkills us
                ON us.SkillId = s.Id
            ORDER BY
                CASE s.Branch
                    WHEN 'Mobility' THEN 1
                    WHEN 'Survival' THEN 2
                    WHEN 'Conditioning' THEN 3
                    ELSE 4
                END,
                s.Id;
            """;

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            skills.Add(new Skill
            {
                Id = reader.GetInt32(0),

                Branch = reader.GetString(1),

                Name = reader.GetString(2),

                Description = reader.GetString(3),

                Details = reader.IsDBNull(4)
                    ? null
                    : reader.GetString(4),

                MaxPoints = reader.GetInt32(5),

                Requires = reader.IsDBNull(6)
                    ? null
                    : reader.GetString(6),

                CurrentPoints = reader.GetInt32(7)
            });
        }

        return skills;
    }

    public async Task SaveSkillPointsAsync(
        int skillId,
        int currentPoints)
    {
        await using var connection =
            new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO UserSkills
                (SkillId, CurrentPoints)
            VALUES
                ($skillId, $currentPoints)
            ON CONFLICT(SkillId)
            DO UPDATE SET
                CurrentPoints = $currentPoints;
            """;

        command.Parameters.AddWithValue(
            "$skillId",
            skillId);

        command.Parameters.AddWithValue(
            "$currentPoints",
            currentPoints);

        await command.ExecuteNonQueryAsync();
    }
}