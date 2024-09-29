using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace GlassExamination
{
    public class DataUtils
    {
        // 更新資料
        // update tableName set column1=value1, column2=value2 where condition
        public static async Task updateDataAsync(string tableName, string condition, Dictionary<string, object> parameters, string conn)
        {
            using (SqlConnection connection = new SqlConnection(conn))
            {
                await connection.OpenAsync();
                string setClause = string.Join(",", parameters.Keys.Select(key => $"{key}=@{key}"));
                string sqlUpdate = $"Update {tableName} set {setClause} where {condition}";
                using (SqlCommand command = new SqlCommand(sqlUpdate, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                    }

                    int rowAffected = await command.ExecuteNonQueryAsync();

                    if (rowAffected > 0)
                    {
                        Console.WriteLine($"{rowAffected} 筆資料更新成功");
                    }
                    else
                    {
                        Console.WriteLine("未找到符合條件的紀錄更新");
                    }
                }
            }

        }

        // 計算select符合where條件有幾個，用來判斷是否找的到資料
        // select count(1) from tableName where condition
        public static async Task<int> queryDataCountAysnc(string tableName, string condition, Dictionary<string, object> parameters, string conn)
        {
            int countResult = 0;
            using (SqlConnection connection = new SqlConnection(conn))
            {
                await connection.OpenAsync();
                string sqlSelect = $"select count(1) from {tableName}";
                if (!string.IsNullOrEmpty(condition))
                {
                    sqlSelect += $" WHERE {condition}";
                }

                using (SqlCommand command = new SqlCommand(sqlSelect, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                        }
                    }

                    object count = await command.ExecuteScalarAsync();
                    if (count != null && int.TryParse(count.ToString(), out int result))
                    {
                        countResult = result;
                    }

                }
            }
            return countResult;
        }


        // 將選擇出來的結果直接放到表格當中
        // select * from tableName where condition
        public static async Task<DataTable> queryDataTableAsync(string tableName, string condition, Dictionary<string, object> parameters, string conn)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(conn))
            {
                await connection.OpenAsync();
                string sqlSelect = $"select * from {tableName}";
                if (!string.IsNullOrEmpty(condition)) // 當 condition 不為空時，添加 WHERE 子句
                {
                    sqlSelect += $" WHERE {condition}";
                }

                using (SqlCommand command = new SqlCommand(sqlSelect, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                        }
                    }

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }

                }
            }
            return dataTable;

        }
    }

}
