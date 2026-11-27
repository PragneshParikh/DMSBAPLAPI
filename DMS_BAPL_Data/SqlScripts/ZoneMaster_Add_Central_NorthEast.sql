/* =====================================================================================================
   ZoneMaster_Add_Central_NorthEast.sql                                         27.11.2026
   Adds two zones - "Central" and "North East" - to dbo.zone_master (6 zones in all with North, South,
   East, West) and moves the dealers of those states into them.

   How the employee form places a dealer in a zone (ZoneMasterRepo.BuildZoneDealersAsync):
     1. the dealer has its own active zone_master row (dealer_id)  -> that zone
     2. otherwise the dealer's state appears in a zone's active rows -> that zone
   So this script:
     STEP 1  shows what will change (read-only)
     STEP 2  deactivates (is_active = 0, nothing is deleted) the rows that keep these states / their
             dealers in another zone, and adds one state row per state for the new zone.
   Run the whole file once as it is (STEP 1 only - STEP 2 is commented out) and check the lists.
   Then remove the /* and */ around STEP 2 and run the WHOLE file again (STEP 2 needs the lists
   built at the top, so do not run STEP 2 on its own). To undo: set is_active = 1 again on the ids
   STEP 2 prints as "deactivated", and set is_active = 0 on the ids it prints as "added".

   ASSUMPTION - please confirm the state lists before running:
     Central    : Madhya Pradesh, Chhattisgarh
     North East : Assam, Arunachal Pradesh, Manipur, Meghalaya, Mizoram, Nagaland, Tripura, Sikkim
   Edit the two lists below if BGauss uses a different split (e.g. Uttar Pradesh in Central).
   zone is VARCHAR(20); use the same spelling style as the existing zones (e.g. "North", "South").
   ===================================================================================================== */

SET NOCOUNT ON;

DECLARE @ZoneStates TABLE (zone VARCHAR(20) NOT NULL, state_name VARCHAR(100) NOT NULL);
INSERT INTO @ZoneStates (zone, state_name) VALUES
    ('Central',    'MADHYA PRADESH'),
    ('Central',    'CHHATTISGARH'),
    ('Central',    'CHATTISGARH'),          -- common alternate spelling
    ('North East', 'ASSAM'),
    ('North East', 'ARUNACHAL PRADESH'),
    ('North East', 'MANIPUR'),
    ('North East', 'MEGHALAYA'),
    ('North East', 'MIZORAM'),
    ('North East', 'NAGALAND'),
    ('North East', 'TRIPURA'),
    ('North East', 'SIKKIM');

/* state_id -> new zone */
DECLARE @Map TABLE (state_id INT PRIMARY KEY, zone VARCHAR(20) NOT NULL, state_name VARCHAR(100));
INSERT INTO @Map (state_id, zone, state_name)
SELECT s.state_id, MIN(zs.zone), MIN(s.state_name)
FROM dbo.states s
JOIN @ZoneStates zs ON UPPER(LTRIM(RTRIM(s.state_name))) = zs.state_name
GROUP BY s.state_id;

/* dealers located in those states (DealerMaster.state holds a state id or a state name) */
DECLARE @DealerZone TABLE (dealer_id INT PRIMARY KEY, zone VARCHAR(20) NOT NULL);
INSERT INTO @DealerZone (dealer_id, zone)
SELECT d.id, MIN(m.zone)
FROM dbo.DealerMaster d
JOIN @Map m
  ON m.state_id = TRY_CAST(LTRIM(RTRIM(d.state)) AS INT)
  OR UPPER(LTRIM(RTRIM(d.state))) = UPPER(LTRIM(RTRIM(m.state_name)))
GROUP BY d.id;

/* ---------------------------------------------------------------- STEP 1: preview (read-only) */
SELECT 'States found' AS [Check], m.zone AS NewZone, m.state_id, m.state_name FROM @Map m ORDER BY m.zone, m.state_name;

SELECT zs.zone, zs.state_name AS [State NOT found in dbo.states - check spelling]
FROM @ZoneStates zs
WHERE NOT EXISTS (SELECT 1 FROM dbo.states s WHERE UPPER(LTRIM(RTRIM(s.state_name))) = zs.state_name);

SELECT 'Dealers that move' AS [Check], dz.zone AS NewZone, d.dealercode, d.compname, d.state, d.city
FROM @DealerZone dz JOIN dbo.DealerMaster d ON d.id = dz.dealer_id
ORDER BY dz.zone, d.compname;

SELECT 'zone_master rows to deactivate' AS [Check], z.id, z.zone AS OldZone, z.dealer_id, z.state_id, z.city_id
FROM dbo.zone_master z
LEFT JOIN dbo.cities c ON c.city_id = z.city_id
LEFT JOIN @Map ms ON ms.state_id = z.state_id
LEFT JOIN @Map mc ON mc.state_id = c.state_id
LEFT JOIN @DealerZone dz ON dz.dealer_id = z.dealer_id
WHERE z.is_active = 1
  AND (   (ms.state_id IS NOT NULL AND z.zone <> ms.zone)
       OR (z.state_id IS NULL AND mc.state_id IS NOT NULL AND z.zone <> mc.zone)
       OR (dz.dealer_id IS NOT NULL AND z.zone <> dz.zone));

/* ---------------------------------------------------------------- STEP 2: apply
   Remove the /* and */ around the block below, then run the whole file. */
/*
BEGIN TRY
    BEGIN TRAN;

    -- 2a. old-zone rows of these states (by state, by city, or by a dealer located there)
    UPDATE z SET z.is_active = 0
    OUTPUT 'deactivated' AS Action, inserted.id, inserted.zone, inserted.dealer_id, inserted.state_id, inserted.city_id
    FROM dbo.zone_master z
    LEFT JOIN dbo.cities c ON c.city_id = z.city_id
    LEFT JOIN @Map ms ON ms.state_id = z.state_id
    LEFT JOIN @Map mc ON mc.state_id = c.state_id
    LEFT JOIN @DealerZone dz ON dz.dealer_id = z.dealer_id
    WHERE z.is_active = 1
      AND (   (ms.state_id IS NOT NULL AND z.zone <> ms.zone)
           OR (z.state_id IS NULL AND mc.state_id IS NOT NULL AND z.zone <> mc.zone)
           OR (dz.dealer_id IS NOT NULL AND z.zone <> dz.zone));

    -- 2b. re-activate an existing inactive state row of the new zone, if there is one
    UPDATE z SET z.is_active = 1
    OUTPUT 'reactivated' AS Action, inserted.id, inserted.zone, inserted.dealer_id, inserted.state_id, inserted.city_id
    FROM dbo.zone_master z
    JOIN @Map m ON m.state_id = z.state_id AND m.zone = z.zone
    WHERE z.is_active = 0 AND z.dealer_id IS NULL AND z.city_id IS NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.zone_master x
                      WHERE x.state_id = m.state_id AND x.zone = m.zone AND x.is_active = 1
                        AND x.dealer_id IS NULL AND x.city_id IS NULL);

    -- 2c. one active state row per state for the new zone
    INSERT INTO dbo.zone_master (zone, state_id, dealer_id, city_id, is_active)
    OUTPUT 'added' AS Action, inserted.id, inserted.zone, inserted.dealer_id, inserted.state_id, inserted.city_id
    SELECT m.zone, m.state_id, NULL, NULL, 1
    FROM @Map m
    WHERE NOT EXISTS (SELECT 1 FROM dbo.zone_master x
                      WHERE x.state_id = m.state_id AND x.zone = m.zone AND x.is_active = 1
                        AND x.dealer_id IS NULL AND x.city_id IS NULL);

    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;

-- result: dealer count per zone (the employee form shows the same after a refresh)
SELECT zone, COUNT(*) AS ActiveRows FROM dbo.zone_master WHERE is_active = 1 GROUP BY zone ORDER BY zone;
*/
