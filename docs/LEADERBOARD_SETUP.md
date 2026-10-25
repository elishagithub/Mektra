# Unity leaderboard connection

Mektra uses the existing Arkcode Solutions Unity project `71cdb2f4-2bd5-4de6-97fa-fb18e864069a` (organization `13469982337887`). The production environment is `42616737-1512-4304-947e-21ce8540fa77`; board `mektra_distance` is named Mektra Global Climb.

The board ranks highest to lowest, updates best score only and has no scheduled reset. Preserve this board and its entries. The missing project linkage in ProjectSettings was restored on 25 October 2026.

The editor probe signs in using an isolated profile and reads the real board without posting scores or deleting accounts. Run `Mektra.Editor.LeaderboardIntegration.Probe` with `-mektraLeaderboardProbe` and without `-quit`; the probe exits automatically. The flag suppresses gameplay bootstrap during the read test. A successful run logs `MEKTRA_GLOBAL_LEADERBOARD_CONNECTED`.

Existing installed store builds still need an update containing the restored project link. Local best scores remain saved when offline.
