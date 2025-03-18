using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using nova_log.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nova_log.DataAccess
{
    public class GoogleSheetService
    {
        private readonly string _spreadsheetId;
        private readonly string _sheetName;
        private readonly SheetsService _sheetsService;


        public GoogleSheetService(string spreadsheetId, string sheetName)
        {
            string jsonKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "novalogging-453514-a30988280f67.json");

            GoogleCredential credential;
            using (var stream = new FileStream(jsonKeyPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(new[] { SheetsService.Scope.Spreadsheets });
            }

            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Nova Log App"
            });

            _spreadsheetId = spreadsheetId;
            _sheetName = sheetName;
        }

      
        public async Task<bool> AppendToSheet(DataTable dataTable)
        {
            try
            {
                var range = $"{_sheetName}!A:{(char)('A' + dataTable.Columns.Count - 1)}";

                var valueRange = new ValueRange
                {
                    Values = new List<IList<object>>()
                };

                foreach (DataRow row in dataTable.Rows)
                {
                    var rowData = new List<object>();
                    foreach (var item in row.ItemArray)
                    {
                        rowData.Add(item?.ToString() ?? ""); 
                    }
                    valueRange.Values.Add(rowData);
                }
                var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;

                var response = await appendRequest.ExecuteAsync();
                return response.Updates != null && response.Updates.UpdatedRows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while appending data to the sheet.", ex);
            }
        }

        public async Task<(DataTable dataTable, Exception? error)> GetTaskListAsDataTableAsync(string columnFrom, string columnTo)
        {
            try
            {
                string range = $"{_sheetName}!{columnFrom}:{columnTo}";
                var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

                var response = await request.ExecuteAsync();
                var values = response.Values;

                DataTable dataTable = new DataTable();

                if (values != null && values.Count > 0)
                {
                    var headerRow = values[0];
                    foreach (var header in headerRow)
                    {
                        dataTable.Columns.Add(header.ToString());
                    }

                    // Add data rows (skip the header row)
                    for (int i = 1; i < values.Count; i++)
                    {
                        var row = values[i];
                        var dataRow = dataTable.NewRow();

                        try
                        {
                            // Fill each column of the row
                            for (int j = 0; j < row.Count && j < dataTable.Columns.Count; j++)
                            {
                                if (row[j] != null)
                                {
                                    // Type conversion based on column index
                                    if (j == 2) // EstimateHours - convert to int
                                    {
                                        dataRow[j] = ParseIntSafely(row[j]);
                                    }
                                    else if (j == 3 || j == 4) // Dates - convert to DateTime
                                    {
                                        dataRow[j] = ParseDateSafely(row[j]);
                                    }
                                    else // Other columns - keep as string
                                    {
                                        dataRow[j] = row[j].ToString();
                                    }
                                }
                            }

                            // Add the row to the DataTable
                            dataTable.Rows.Add(dataRow);
                        }
                        catch (Exception ex)
                        {
                            return (new DataTable(), ex);
                        }
                    }
                }
                else
                {
                    return (new DataTable(), new NullReferenceException("No data fetch from google sheet"));
                }
                return (dataTable, null);
            }
            catch (Exception ex)
            {
                return (new DataTable(), ex);
            }
        }



        private int ParseIntSafely(object value)
        {
            if (value == null) return 0;
            return int.TryParse(value.ToString(), out int result) ? result : 0;
        }

        private DateTime ParseDateSafely(object value)
        {
            if (value == null) return DateTime.MinValue;
            return DateTime.TryParse(value.ToString(), out DateTime result) ? result : DateTime.MinValue;
        }
    }
}
