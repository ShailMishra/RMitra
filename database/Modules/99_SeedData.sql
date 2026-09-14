USE [HomelyFood];
GO

IF NOT EXISTS (SELECT 1 FROM mstCuisines)
BEGIN
    INSERT INTO mstCuisines (Code, Name) VALUES
        (N'NORTH_INDIAN', N'North Indian'),
        (N'SOUTH_INDIAN', N'South Indian'),
        (N'GUJARATI', N'Gujarati'),
        (N'RAJASTHANI', N'Rajasthani'),
        (N'BENGALI', N'Bengali'),
        (N'PUNJABI', N'Punjabi'),
        (N'MAHARASHTRIAN', N'Maharashtrian'),
        (N'CHINESE', N'Chinese'),
        (N'CONTINENTAL', N'Continental'),
        (N'STREET_FOOD', N'Street Food'),
        (N'SWEETS', N'Sweets'),
        (N'OTHER', N'Other');
END
GO

IF NOT EXISTS (SELECT 1 FROM mstPlans)
BEGIN
    INSERT INTO mstPlans (PlanCode, Audience, Name, Price, DurationDays) VALUES
        (N'HOMELY_PLUS_MONTHLY', N'CUSTOMER', N'HomelyPlus Monthly', 149, 30),
        (N'HOMELY_PLUS_QUARTERLY', N'CUSTOMER', N'HomelyPlus Quarterly', 399, 90),
        (N'HOMELY_PLUS_ANNUAL', N'CUSTOMER', N'HomelyPlus Annual', 999, 365),
        (N'FEATURED_MONTHLY', N'KITCHEN', N'Featured Kitchen Monthly', 499, 30);
END
GO

IF NOT EXISTS (SELECT 1 FROM mstUsers WHERE MobileNumber = N'9999999999' AND Role = N'ADMIN')
BEGIN
    INSERT INTO mstUsers (Id, UserCode, FullName, MobileNumber, Email, Role, Status, CreatedAt, UpdatedAt)
    VALUES (
        '11111111-1111-1111-1111-111111111111',
        N'ADM10001',
        N'HOMELY Admin',
        N'9999999999',
        N'admin@homely.local',
        N'ADMIN',
        1,
        SYSUTCDATETIME(),
        SYSUTCDATETIME()
    );
END
GO
