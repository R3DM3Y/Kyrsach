using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kyrsach.Services;

public static class ValidationService
{
    public static (bool IsValid, string? Error) ValidateName(string name, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (false, $"{fieldName} обязателен для заполнения");
        
        if (name.Length > 50)
            return (false, $"{fieldName} должен быть не длиннее 50 символов");
        
        if (!Regex.IsMatch(name, @"^[\p{L}\s\-]+$"))
            return (false, $"{fieldName} содержит недопустимые символы");
        
        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return (false, "Телефон обязателен для заполнения");
        
        var cleanPhone = new string(phone.Where(char.IsDigit).ToArray());
        
        if (cleanPhone.Length < 10)
            return (false, "Телефон должен содержать минимум 10 цифр");
        
        if (cleanPhone.Length > 15)
            return (false, "Телефон слишком длинный");
        
        return (true, null);
    }

    public static (bool IsValid, string? Error) ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (true, null);
        
        if (email.Length > 100)
            return (false, "Email слишком длинный");
        
        try
        {
            var mailAddress = new System.Net.Mail.MailAddress(email);
            return mailAddress.Address == email ? (true, null) : (false, "Некорректный email");
        }
        catch
        {
            return (false, "Некорректный формат email");
        }
    }

    public static (bool IsValid, string? Error) ValidateBirthDate(DateOnly? birthDate)
    {
        if (!birthDate.HasValue)
            return (true, null);
        
        var today = DateOnly.FromDateTime(DateTime.Today);
        var minDate = today.AddYears(-120);
        var maxDate = today.AddYears(-14);
        
        if (birthDate > maxDate)
            return (false, "Клиент должен быть старше 14 лет");
        
        if (birthDate < minDate)
            return (false, "Некорректная дата рождения");
        
        return (true, null);
    }
}