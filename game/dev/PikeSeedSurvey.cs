using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fish;

public static class PikeSeedSurvey
{
    public static void Run(SceneTree tree)
    {
        var rows = new List<(int seed, float score, float shallow, float middle, float deep, float support)>();
        var wood = new (float x, float z, float length)[]
        {
            (11.3f, 19f, 14.9f), (-14.1f, 19.2f, 17.6f),
            (-8.1f, 2.6f, 10.8f), (9.1f, 3.2f, 10.9f),
            (18.5f, 0f, 10f), (-18.5f, 0f, 10f),
            (-8f, -17.8f, 7.7f), (8.2f, -18.2f, 9.2f),
            (1f, -24.5f, 10.5f), (12.8f, -25f, 12.4f),
        };
        for (var seed = 5202; seed < 5302; seed++)
        {
            var basin = new GeneratedBasin((ulong)seed, pond: true);
            float Depth(float x, float z) => GeneratedBasin.WaterSurfaceY - basin.HeightAt(x, z);
            var shallow = Depth(0, 23); var middle = Depth(0, 2); var deep = Depth(0, -24);
            var maxSupport = wood.Max(w => Math.Max(
                Math.Abs(basin.HeightAt(w.x, w.z - w.length * .35f) - basin.HeightAt(w.x, w.z)),
                Math.Abs(basin.HeightAt(w.x, w.z + w.length * .35f) - basin.HeightAt(w.x, w.z))));
            var score = Math.Abs(shallow - 4.5f) + Math.Abs(middle - 8f) +
                Math.Abs(deep - 11f) + 3f * Math.Max(0, shallow - middle + 1f) +
                3f * Math.Max(0, middle - deep + 1f) + 2f * maxSupport;
            rows.Add((seed, score, shallow, middle, deep, (float)maxSupport));
        }
        foreach (var row in rows.OrderBy(r => r.score).Take(12))
            GD.Print($"PIKE_SEED_CANDIDATE seed={row.seed} score={row.score:F2} depth={row.shallow:F2}/{row.middle:F2}/{row.deep:F2} support={row.support:F2}");
        GD.Print("PIKE_SEED_SURVEY_OK candidates=100");
        tree.Quit();
    }
}
