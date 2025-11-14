using System.Globalization;

namespace Pro4Soft.BackgroundWorker.Readers.Edi940;

public class Edi940Reader
{
    public Edi940Document ReadFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var doc = new Edi940Document();
        
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed == "BEGIN" || trimmed == "END")
                continue;
            
            if (trimmed.StartsWith("PARTNER"))
                doc.Partner = GetValue(trimmed);
            else if (trimmed.StartsWith("ROUTE"))
                doc.Route = GetValue(trimmed);
            else if (trimmed.StartsWith("COMPANY"))
                doc.Company = GetValue(trimmed);
            else if (trimmed.StartsWith("SET"))
                doc.Set = GetValue(trimmed);
            else if (trimmed.StartsWith("I940_1020_004010"))
                ParseHeaderSegment(trimmed, doc);
            else if (trimmed.StartsWith("I940_1040_004010"))
                ParseLocationSegment(trimmed, doc);
            else if (trimmed.StartsWith("I940_1110_004010"))
                ParseDateSegment(trimmed, doc);
            else if (trimmed.StartsWith("I940_2020_004010"))
                ParseLineItem(trimmed, doc);
        }
        
        return doc;
    }
    
    private string GetValue(string line)
    {
        var parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? parts[1].Trim() : string.Empty;
    }
    
    private void ParseHeaderSegment(string line, Edi940Document doc)
    {
        if (doc.Header == null)
            doc.Header = new Edi940Header();
        
        var data = line.Substring(20).TrimStart();
        
        var orderIdx = data.IndexOf("1020N", StringComparison.Ordinal);
        if (orderIdx >= 0)
        {
            var orderStart = orderIdx + 5;
            var orderPart = data.Substring(orderStart);
            var parts = orderPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length > 0)
                doc.Header.OrderNumber = parts[0];
            if (parts.Length > 1)
                doc.Header.RouteName = parts[1];
            if (parts.Length > 2)
                doc.Header.Reference = parts[2];
        }
    }
    
    private void ParseLocationSegment(string line, Edi940Document doc)
    {
        if (doc.Header == null)
            doc.Header = new Edi940Header();
        
        var data = line.Substring(20).TrimStart();
        doc.Header.ShipFromLocation = data.Substring(4).Trim();
    }
    
    private void ParseDateSegment(string line, Edi940Document doc)
    {
        if (doc.Header == null)
            doc.Header = new Edi940Header();
        
        var data = line.Substring(20).TrimStart();
        
        var dateIdx = data.IndexOf("1110", StringComparison.Ordinal);
        if (dateIdx >= 0)
        {
            var dateStart = dateIdx + 4;
            var remaining = data.Substring(dateStart);
            
            if (remaining.Length >= 10)
            {
                var dateStr = remaining.Substring(2, 8);
                if (DateTime.TryParseExact(dateStr, "yyyyMMdd", 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    doc.Header.ShipDate = date;
                }
            }
        }
    }
    
    private void ParseLineItem(string line, Edi940Document doc)
    {
        var data = line.Substring(20).TrimStart();
        var item = new Edi940LineItem();
        
        var qtyIdx = data.IndexOf("2020", StringComparison.Ordinal);
        if (qtyIdx >= 0)
        {
            var qtyStart = qtyIdx + 4;
            var remaining = data.Substring(qtyStart);
            var parts = remaining.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length > 0 && decimal.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var qty))
                item.Quantity = qty;
            
            if (parts.Length > 1)
                item.UnitOfMeasure = parts[1];
            
            if (parts.Length > 2)
            {
                var productIdx = remaining.IndexOf("VN", StringComparison.Ordinal);
                if (productIdx >= 0)
                    item.ProductCode = remaining.Substring(productIdx + 2).Trim();
            }
        }
        
        doc.LineItems.Add(item);
    }
}