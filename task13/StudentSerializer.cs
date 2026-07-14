using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class StudentSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    static StudentSerializer()
    {
        Options.Converters.Add(new JsonDateTimeConverter());
    }

    public static string Serialize(Student student)
        => JsonSerializer.Serialize(student, Options);

    public static Student Deserialize(string json)
    {
        var student = JsonSerializer.Deserialize<Student>(json, Options)
            ?? throw new InvalidOperationException("Десериализация вернула null");
        Validate(student);
        return student;
    }

    public static void SaveToFile(Student student, string path)
        => File.WriteAllText(path, Serialize(student));

    public static Student LoadFromFile(string path)
        => Deserialize(File.ReadAllText(path));

    private static void Validate(Student student)
    {
        if (student == null)
            throw new ArgumentNullException(nameof(student));
        if (string.IsNullOrWhiteSpace(student.FirstName))
            throw new InvalidOperationException("FirstName не может быть пустым");
        if (string.IsNullOrWhiteSpace(student.LastName))
            throw new InvalidOperationException("LastName не может быть пустым");
        if (student.BirthDate > DateTime.Now)
            throw new InvalidOperationException("Дата рождения не может быть в будущем");
        if (student.BirthDate < new DateTime(1900, 1, 1))
            throw new InvalidOperationException("Дата рождения слишком старая");
        if (student.Grades != null)
        {
            foreach (var grade in student.Grades)
            {
                if (grade.Grade < 1 || grade.Grade > 5)
                    throw new InvalidOperationException($"Оценка по предмету '{grade.Name}' должна быть от 1 до 5");
            }
        }
    }
}

public class JsonDateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "dd.MM.yyyy";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateStr = reader.GetString();
        if (DateTime.TryParseExact(dateStr, Format, null, System.Globalization.DateTimeStyles.None, out var date))
            return date;
        throw new JsonException($"Неверный формат даты. Ожидается: {Format}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));
}
