# Initial working budgets

These ranges guide consistent construction and performance reviews. They are **tunable starting points**, not permanent caps or Roadmap Gate evidence. Count exported triangles after modifiers/triangulation, and measure representative scene density in Godot before locking production targets.

| Asset | Working triangles |
| --- | ---: |
| Small/background fish | 800–2,500 |
| Hero/player fish | 2,000–6,000 |
| Small coral | 100–800 |
| Hero coral | 500–3,000 |
| Small rock | 50–500 |
| Hero rock | 500–3,000 |
| Grass/small plants | 20–300 |

Terrain varies with chunk size, distance and collision needs; set a per-chunk target after a representative view is measured. Start with one to three simple materials for a small asset and a few shared palette materials for a hero; explain extra slots. Prefer flat/vertex color or compact textures to high-resolution maps, and avoid extra transparent layers unless their silhouette benefit survives in Godot. Track material count, texture dimensions and actual draw cost in the candidate record. A hero may exceed these numbers with documented visual value and measured performance.
