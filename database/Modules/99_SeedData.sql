USE [RasoiMitra];
GO

/* Lookup / master data required by the Homely APIs */

IF NOT EXISTS (SELECT 1 FROM masRoles)
BEGIN
    INSERT INTO masRoles (Code, Name, DisplayOrder) VALUES
        (N'CUSTOMER', N'Customer', 1),
        (N'KITCHEN_OWNER', N'Kitchen Owner', 2),
        (N'RIDER', N'Rider', 3),
        (N'ADMIN', N'Admin', 4);
END
GO

IF NOT EXISTS (SELECT 1 FROM masAuthPurposes)
BEGIN
    INSERT INTO masAuthPurposes (Code, Name, DisplayOrder) VALUES
        (N'CUSTOMER', N'Customer registration / login', 1),
        (N'KITCHEN_OWNER', N'Kitchen owner registration / login', 2),
        (N'RIDER', N'Rider registration / login', 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masUserStatuses)
BEGIN
    INSERT INTO masUserStatuses (Code, Name, StatusValue, DisplayOrder) VALUES
        (N'ACTIVE', N'Active', 1, 1),
        (N'INACTIVE', N'Inactive', 2, 2),
        (N'BLOCKED', N'Blocked', 3, 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masKitchenTypes)
BEGIN
    INSERT INTO masKitchenTypes (Code, Name, DisplayOrder) VALUES
        (N'HOMEMADE', N'Homemade kitchen', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM masKitchenStatuses)
BEGIN
    INSERT INTO masKitchenStatuses (Code, Name, DisplayOrder) VALUES
        (N'DRAFT', N'Draft', 1),
        (N'MOBILE_VERIFIED', N'Mobile verified', 2),
        (N'SUBMITTED', N'Submitted', 3),
        (N'DOCUMENT_VERIFICATION_PENDING', N'Document verification pending', 4),
        (N'UNDER_REVIEW', N'Under review', 5),
        (N'ADDITIONAL_DOCUMENTS_REQUIRED', N'Additional documents required', 6),
        (N'RESUBMITTED', N'Resubmitted', 7),
        (N'REJECTED', N'Rejected', 8),
        (N'APPROVED', N'Approved', 9),
        (N'MENU_SETUP', N'Menu setup', 10),
        (N'ACTIVE', N'Active', 11);
END
GO

IF NOT EXISTS (SELECT 1 FROM masDocumentTypes)
BEGIN
    INSERT INTO masDocumentTypes (Code, Name, Audience, IsRequired, DisplayOrder) VALUES
        (N'KITCHEN_PHOTO', N'Kitchen photo', N'KITCHEN', 1, 1),
        (N'PAN_CARD', N'PAN card', N'KITCHEN', 1, 2),
        (N'FSSAI_LICENSE', N'FSSAI license', N'KITCHEN', 0, 3),
        (N'AADHAAR', N'Aadhaar', N'RIDER', 1, 4),
        (N'DRIVING_LICENCE', N'Driving licence', N'RIDER', 1, 5),
        (N'VEHICLE_RC', N'Vehicle RC', N'RIDER', 1, 6),
        (N'SELFIE', N'Selfie', N'RIDER', 1, 7);
END
GO

IF NOT EXISTS (SELECT 1 FROM masCuisines)
BEGIN
    INSERT INTO masCuisines (Code, Name, DisplayOrder) VALUES
        (N'NORTH_INDIAN', N'North Indian', 1),
        (N'SOUTH_INDIAN', N'South Indian', 2),
        (N'GUJARATI', N'Gujarati', 3),
        (N'RAJASTHANI', N'Rajasthani', 4),
        (N'BENGALI', N'Bengali', 5),
        (N'PUNJABI', N'Punjabi', 6),
        (N'MAHARASHTRIAN', N'Maharashtrian', 7),
        (N'CHINESE', N'Chinese', 8),
        (N'CONTINENTAL', N'Continental', 9),
        (N'STREET_FOOD', N'Street Food', 10),
        (N'SWEETS', N'Sweets', 11),
        (N'OTHER', N'Other', 12);
END
GO

IF NOT EXISTS (SELECT 1 FROM masMenuItemStatuses)
BEGIN
    INSERT INTO masMenuItemStatuses (Code, Name, DisplayOrder) VALUES
        (N'DRAFT', N'Draft', 1),
        (N'PUBLISHED', N'Published', 2),
        (N'DELETED', N'Deleted', 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masAddressLabels)
BEGIN
    INSERT INTO masAddressLabels (Code, Name, DisplayOrder) VALUES
        (N'HOME', N'Home', 1),
        (N'WORK', N'Work', 2),
        (N'OTHER', N'Other', 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masOrderStatuses)
BEGIN
    INSERT INTO masOrderStatuses (Code, Name, DisplayOrder) VALUES
        (N'PAYMENT_PENDING', N'Payment pending', 1),
        (N'PAYMENT_FAILED', N'Payment failed', 2),
        (N'PLACED', N'Placed', 3),
        (N'ACCEPTED', N'Accepted', 4),
        (N'PREPARING', N'Preparing', 5),
        (N'READY', N'Ready', 6),
        (N'OUT_FOR_DELIVERY', N'Out for delivery', 7),
        (N'DELIVERED', N'Delivered', 8),
        (N'CANCELLED', N'Cancelled', 9),
        (N'REJECTED', N'Rejected', 10),
        (N'REFUNDED', N'Refunded', 11);
END
GO

IF NOT EXISTS (SELECT 1 FROM masPaymentMethods)
BEGIN
    INSERT INTO masPaymentMethods (Code, Name, DisplayOrder) VALUES
        (N'UPI', N'UPI', 1),
        (N'CARD', N'Card', 2),
        (N'NETBANKING', N'Net banking', 3),
        (N'WALLET', N'Wallet', 4),
        (N'COD', N'Cash on delivery', 5);
END
GO

IF NOT EXISTS (SELECT 1 FROM masPaymentStatuses)
BEGIN
    INSERT INTO masPaymentStatuses (Code, Name, DisplayOrder) VALUES
        (N'PENDING', N'Pending', 1),
        (N'SUCCESS', N'Success', 2),
        (N'FAILED', N'Failed', 3),
        (N'REFUNDED', N'Refunded', 4);
END
GO

IF NOT EXISTS (SELECT 1 FROM masDeliveryModes)
BEGIN
    INSERT INTO masDeliveryModes (Code, Name, DisplayOrder) VALUES
        (N'RIDER', N'Rider delivery', 1),
        (N'KITCHEN_SELF', N'Kitchen self delivery', 2);
END
GO

IF NOT EXISTS (SELECT 1 FROM masVehicleTypes)
BEGIN
    INSERT INTO masVehicleTypes (Code, Name, DisplayOrder) VALUES
        (N'BIKE', N'Bike', 1),
        (N'SCOOTER', N'Scooter', 2),
        (N'CYCLE', N'Cycle', 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masRiderStatuses)
BEGIN
    INSERT INTO masRiderStatuses (Code, Name, DisplayOrder) VALUES
        (N'DRAFT', N'Draft', 1),
        (N'SUBMITTED', N'Submitted', 2),
        (N'APPROVED', N'Approved', 3),
        (N'REJECTED', N'Rejected', 4);
END
GO

IF NOT EXISTS (SELECT 1 FROM masAssignmentStatuses)
BEGIN
    INSERT INTO masAssignmentStatuses (Code, Name, DisplayOrder) VALUES
        (N'OFFERED', N'Offered', 1),
        (N'ACCEPTED', N'Accepted', 2),
        (N'DECLINED', N'Declined', 3),
        (N'EXPIRED', N'Expired', 4);
END
GO

IF NOT EXISTS (SELECT 1 FROM masTripStatuses)
BEGIN
    INSERT INTO masTripStatuses (Code, Name, DisplayOrder) VALUES
        (N'ASSIGNED', N'Assigned', 1),
        (N'ARRIVED_KITCHEN', N'Arrived at kitchen', 2),
        (N'PICKED_UP', N'Picked up', 3),
        (N'DELIVERED', N'Delivered', 4);
END
GO

IF NOT EXISTS (SELECT 1 FROM masSettlementStatuses)
BEGIN
    INSERT INTO masSettlementStatuses (Code, Name, DisplayOrder) VALUES
        (N'PENDING', N'Pending', 1),
        (N'PAID', N'Paid', 2),
        (N'FAILED', N'Failed', 3);
END
GO

IF NOT EXISTS (SELECT 1 FROM masPlans)
BEGIN
    INSERT INTO masPlans (PlanCode, Audience, Name, Price, DurationDays) VALUES
        (N'HOMELY_PLUS_MONTHLY', N'CUSTOMER', N'HomelyPlus Monthly', 149, 30),
        (N'HOMELY_PLUS_QUARTERLY', N'CUSTOMER', N'HomelyPlus Quarterly', 399, 90),
        (N'HOMELY_PLUS_ANNUAL', N'CUSTOMER', N'HomelyPlus Annual', 999, 365),
        (N'FEATURED_MONTHLY', N'KITCHEN', N'Featured Kitchen Monthly', 499, 30);
END
GO

IF NOT EXISTS (SELECT 1 FROM tblUsers WHERE MobileNumber = N'9999999999' AND Role = N'ADMIN')
BEGIN
    INSERT INTO tblUsers (Id, UserCode, FullName, MobileNumber, Email, Role, Status, CreatedAt, UpdatedAt)
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
