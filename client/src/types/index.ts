export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data: T;
  errors?: string[];
  timestampUtc: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phoneNumber: string;
  isActive: boolean;
  roles: string[];
  lastLoginAtUtc?: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  user: User;
}

export interface Tower {
  id: number;
  towerCode: string;
  towerName: string;
  numberOfFloors: number;
  totalFlats: number;
  description?: string;
  isActive: boolean;
  occupiedFlatsCount: number;
  vacantFlatsCount: number;
}

export interface FlatOwnerSummary {
  residentId: number;
  fullName: string;
  email: string;
  mobileNumber: string;
  ownershipPercentage: number;
  isPrimaryOwner: boolean;
  startDateUtc: string;
}

export interface FlatOccupantSummary {
  residentId: number;
  fullName: string;
  mobileNumber: string;
  occupancyType: string;
  moveInDateUtc: string;
}

export interface Flat {
  id: number;
  towerId: number;
  towerName: string;
  towerCode: string;
  flatNumber: string;
  floorNumber: number;
  areaSqFt: number;
  areaMeasurementType: string;
  ownershipStatus: string;
  occupancyStatus: string;
  isActive: boolean;
  owners: FlatOwnerSummary[];
  occupants: FlatOccupantSummary[];
  allocatedParkingLots: string[];
}

export interface Resident {
  id: number;
  userId?: string;
  fullName: string;
  email: string;
  mobileNumber: string;
  residentType: string;
  emergencyContactName?: string;
  emergencyContactNumber?: string;
  correspondenceAddress?: string;
  status: string;
  associatedFlats: string[];
}

export interface ParkingLot {
  id: number;
  lotNumber: string;
  towerId?: number;
  towerName?: string;
  parkingType: string;
  floorOrBasementLevel: string;
  status: string;
  remarks?: string;
  isActive: boolean;
  allocatedFlatId?: number;
  allocatedFlatNumber?: string;
  allocatedResidentName?: string;
  vehicleNumber?: string;
  allocationStartDateUtc?: string;
}

export interface Employee {
  id: number;
  employeeCode: string;
  userId?: string;
  firstName: string;
  lastName: string;
  fullName: string;
  designation: string;
  department: string;
  mobileNumber: string;
  email: string;
  address: string;
  joiningDate: string;
  employmentType: string;
  monthlySalary?: number;
  emergencyContactName?: string;
  emergencyContactNumber?: string;
  status: string;
  isActive: boolean;
}

export interface VendorCategory {
  id: number;
  name: string;
  description?: string;
  categoryType: number;
}

export interface Vendor {
  id: number;
  vendorName: string;
  categoryId: number;
  categoryName: string;
  contactPerson: string;
  mobileNumber: string;
  email: string;
  address: string;
  taxRegistrationNumber?: string;
  paymentTerms?: string;
  serviceStatus: string;
  isActive: boolean;
  activeContractsCount: number;
  nextContractExpiryDate?: string;
}

export interface VendorContract {
  id: number;
  vendorId: number;
  vendorName: string;
  contractTitle: string;
  contractNumber: string;
  startDate: string;
  endDate: string;
  renewalReminderDate?: string;
  contractValue: number;
  status: string;
  scopeOfWork?: string;
}

export interface AssetCategory {
  id: number;
  name: string;
  description?: string;
}

export interface Asset {
  id: number;
  assetCode: string;
  assetName: string;
  categoryId: number;
  categoryName: string;
  description?: string;
  manufacturer?: string;
  modelNumber?: string;
  serialNumber?: string;
  purchaseDate: string;
  purchaseCost: number;
  warrantyExpiryDate?: string;
  towerId?: number;
  towerName?: string;
  specificLocation?: string;
  assignedVendorId?: number;
  assignedVendorName?: string;
  condition: string;
  status: string;
  lastMaintenanceDate?: string;
  nextScheduledMaintenanceDate?: string;
  isActive: boolean;
}

export interface AssetMaintenanceRecord {
  id: number;
  assetId: number;
  assetName: string;
  maintenanceDate: string;
  maintenanceType: string;
  performedBy: string;
  vendorId?: number;
  vendorName?: string;
  costIncurred: number;
  findingsAndWorkDone: string;
  partsReplaced?: string;
  nextRecommendedServiceDate?: string;
}

export interface PurchaseRequestItem {
  id: number;
  itemDescription: string;
  quantity: number;
  unitOfMeasure: string;
  estimatedUnitPrice: number;
  totalEstimatedPrice: number;
  relatedAssetId?: number;
  relatedAssetName?: string;
}

export interface PurchaseApproval {
  id: number;
  approverId: string;
  approverName: string;
  approverRole: string;
  stepOrder: number;
  decision: string;
  comments: string;
  decisionDateUtc: string;
}

export interface PurchaseRequest {
  id: string;
  requestNumber: string;
  title: string;
  description: string;
  requesterId: string;
  requesterName: string;
  vendorId?: number;
  vendorName?: string;
  estimatedTotalAmount: number;
  taxAndOtherCharges: number;
  grandTotal: number;
  budgetCategory: string;
  requiredDeliveryDate: string;
  status: string;
  currentApprovalStep: number;
  currentAssignedRole?: string;
  createdAtUtc: string;
  rowVersion?: string | null;
  items: PurchaseRequestItem[];
  approvals: PurchaseApproval[];
  purchaseOrderId?: string;
  purchaseOrderNumber?: string;
}

export interface PurchaseOrderItem {
  id: number;
  itemDescription: string;
  quantity: number;
  unitOfMeasure: string;
  unitPrice: number;
  totalPrice: number;
}

export interface PurchaseOrder {
  id: string;
  poNumber: string;
  purchaseRequestId: string;
  purchaseRequestNumber: string;
  vendorId: number;
  vendorName: string;
  orderDateUtc: string;
  expectedDeliveryDate?: string;
  totalAmount: number;
  taxAmount: number;
  grandTotal: number;
  deliveryStatus: string;
  termsAndConditions?: string;
  items: PurchaseOrderItem[];
}

export interface DashboardMetrics {
  totalTowers: number;
  totalFlats: number;
  occupiedFlats: number;
  vacantFlats: number;
  occupancyRatePercentage: number;
  totalParkingLots: number;
  allocatedParkingLots: number;
  availableParkingLots: number;
  totalResidents: number;
  totalEmployees: number;
  activeVendors: number;
  contractsExpiringIn30Days: number;
  totalAssets: number;
  assetsDueForMaintenance: number;
  pendingPurchaseRequests: number;
  myPendingApprovalsCount: number;
  approvedPurchaseOrdersThisMonth: number;
  totalApprovedSpendThisMonth: number;
  recentActivities: {
    title: string;
    description: string;
    timestampUtc: string;
    type: string;
  }[];
}

export interface AuditLog {
  id: number;
  entityName: string;
  entityId: string;
  action: string;
  oldValues?: string;
  newValues?: string;
  userId?: string;
  userEmail?: string;
  timestampUtc: string;
}
