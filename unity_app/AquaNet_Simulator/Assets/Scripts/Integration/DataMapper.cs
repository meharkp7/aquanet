using System;
using System.Collections.Generic;

[Serializable]
public class APIResponse
{
    public int retries;
    public float risk_score;
    public List<string> selected_nodes;
}

public class DataMapper
{
    public static List<int> MapPath(APIResponse res)
    {
        List<int> path = new List<int>();

        foreach (var n in res.selected_nodes)
        {
            string[] parts = n.Split('_');
            path.Add(int.Parse(parts[1]));
        }

        return path;
    }
}