using System.Text.Json;

namespace API.Extensions;
public static class JsonExtensions
{
    
    extension<TEntity>(TEntity entity) where TEntity : notnull
    {
        public string ToJson() => JsonSerializer.Serialize(entity);        
        public TEntity FromJson(string json)
        {
            return JsonSerializer.Deserialize<TEntity>(json)!;
        }

        public TEntity Copy<T>(T obj)
        {
            if(obj == null)
            {
                throw new ArgumentNullException(nameof(obj));                
            }

            string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });            
            return JsonSerializer.Deserialize<TEntity>(json)!;
        }
    }
    
}
