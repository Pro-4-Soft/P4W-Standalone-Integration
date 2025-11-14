using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pro4Soft.BackgroundWorker.Dto.P4W.Entities
{
    public class ClientInvoiceP4
    {
        [JsonProperty("client")]
        public ClientP4 Client { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fromName")]
        public string FromName { get; set; }
        [JsonProperty("fromAddress1")]
        public string FromAddress1 { get; set; }
        [JsonProperty("fromAddress2")]
        public string FromAddress2 { get; set; }
        [JsonProperty("fromCity")]
        public string FromCity { get; set; }
        [JsonProperty("fromStateProvince")]
        public string FromStateProvince { get; set; }
        [JsonProperty("fromZipPostalCode")]
        public string FromZipPostalCode { get; set; }
        [JsonProperty("fromCountry")]
        public string FromCountry { get; set; }

        [JsonProperty("billToName")]
        public string BillToName { get; set; }
        [JsonProperty("billToAddress1")]
        public string BillToAddress1 { get; set; }
        [JsonProperty("billToAddress2")]
        public string BillToAddress2 { get; set; }
        [JsonProperty("billToCity")]
        public string BillToCity { get; set; }
        [JsonProperty("billToStateProvince")]
        public string BillToStateProvince { get; set; }
        [JsonProperty("billToZipPostalCode")]
        public string BillToZipPostalCode { get; set; }
        [JsonProperty("billToCountry")]
        public string BillToCountry { get; set; }

        [JsonProperty("lines")]
        public List<ClientInvoiceLineP4> Lines { get; set; } = new();

        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }
        [JsonProperty("clientId")]
        public Guid ClientId { get; set; }
        [JsonProperty("startPeriod")]
        public DateTime? StartPeriod { get; set; }
        [JsonProperty("endPeriod")]
        public DateTime? EndPeriod { get; set; }
        [JsonProperty("postingDate")]
        public DateTime? PostingDate { get; set; }
        [JsonProperty("subTotal")]
        public decimal? SubTotal { get; set; }
        [JsonProperty("total")]
        public decimal? Total { get; set; }
        [JsonProperty("dateCreated")]
        public DateTime? DateCreated { get; set; }
    }

    public class ClientInvoiceLineP4
    {
        [JsonProperty("lineNumber")]
        public int LineNumber { get; set; }

        [JsonProperty("item")]
        public string Item { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("additionalCharges")]
        public string AdditionalCharges { get; set; }

        [JsonProperty("rate")]
        public decimal Rate { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }
    }
}
