import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Chip,
  Button,
  TextField,
  Pagination,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  MenuItem,
  Alert,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import BuildIcon from '@mui/icons-material/Build';
import HistoryIcon from '@mui/icons-material/History';
import api from '../api/client';
import { Asset, AssetCategory, AssetMaintenanceRecord, Tower, ApiResponse, PagedResult } from '../types';
import { useAuth } from '../context/AuthContext';

export const AssetsPage: React.FC = () => {
  const { hasAnyRole } = useAuth();
  const canManage = hasAnyRole(['System Administrator', 'Secretary', 'Maintenance Staff']);

  const [assets, setAssets] = useState<Asset[]>([]);
  const [categories, setCategories] = useState<AssetCategory[]>([]);
  const [towers, setTowers] = useState<Tower[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // Maintenance Log Modal
  const [openLogModal, setOpenLogModal] = useState(false);
  const [selectedAsset, setSelectedAsset] = useState<Asset | null>(null);
  const [maintenanceHistory, setMaintenanceHistory] = useState<AssetMaintenanceRecord[]>([]);
  const [maintenanceForm, setMaintenanceForm] = useState({
    performedBy: '',
    costIncurred: 0,
    findingsAndWorkDone: '',
    partsReplaced: '',
    nextServiceDate: '',
  });

  // Add Asset Modal
  const [openAddModal, setOpenAddModal] = useState(false);
  const [newAsset, setNewAsset] = useState({
    assetCode: '',
    assetName: '',
    categoryId: 1,
    manufacturer: '',
    modelNumber: '',
    serialNumber: '',
    purchaseDate: new Date().toISOString().split('T')[0],
    purchaseCost: 50000,
    towerId: 1,
    specificLocation: '',
  });

  const fetchAssets = async () => {
    try {
      setLoading(true);
      const res = await api.get<ApiResponse<PagedResult<Asset>>>(`/assets?page=${page}&pageSize=10`);
      if (res.data?.success) {
        setAssets(res.data.data.items);
        setTotalPages(res.data.data.totalPages);
        setTotalCount(res.data.data.totalCount);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchMetadata = async () => {
    try {
      const [catRes, towRes] = await Promise.all([
        api.get<ApiResponse<AssetCategory[]>>('/assets/categories'),
        api.get<ApiResponse<Tower[]>>('/towers'),
      ]);
      if (catRes.data?.success) setCategories(catRes.data.data);
      if (towRes.data?.success) setTowers(towRes.data.data);
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchMetadata();
  }, []);

  useEffect(() => {
    fetchAssets();
  }, [page]);

  const handleOpenMaintenance = async (asset: Asset) => {
    setSelectedAsset(asset);
    try {
      const res = await api.get<ApiResponse<AssetMaintenanceRecord[]>>(`/assets/${asset.id}/maintenance`);
      if (res.data?.success) {
        setMaintenanceHistory(res.data.data);
        setOpenLogModal(true);
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleSaveMaintenance = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedAsset) return;

    try {
      const payload = {
        assetId: selectedAsset.id,
        maintenanceDate: new Date().toISOString(),
        maintenanceType: 2, // PreventativeMaintenance
        performedBy: maintenanceForm.performedBy,
        costIncurred: maintenanceForm.costIncurred,
        findingsAndWorkDone: maintenanceForm.findingsAndWorkDone,
        partsReplaced: maintenanceForm.partsReplaced,
        nextRecommendedServiceDate: maintenanceForm.nextServiceDate ? new Date(maintenanceForm.nextServiceDate).toISOString() : null,
      };

      const res = await api.post(`/assets/${selectedAsset.id}/maintenance`, payload);
      if (res.data?.success) {
        setOpenLogModal(false);
        fetchAssets();
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleCreateAsset = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await api.post('/assets', newAsset);
      if (res.data?.success) {
        setOpenAddModal(false);
        fetchAssets();
      }
    } catch (err) {
      console.error(err);
    }
  };

  const getConditionColor = (cond: string) => {
    switch (cond) {
      case 'Good':
      case 'Excellent': return 'success';
      case 'NeedsMaintenance': return 'warning';
      case 'UnderRepair': return 'error';
      default: return 'default';
    }
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
            Asset & Equipment Inventory ({totalCount} assets)
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Lifts, diesel generators, water booster pumps, CCTV arrays, and fire suppression systems.
          </Typography>
        </Box>
        {canManage && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpenAddModal(true)} sx={{ bgcolor: '#1e3a8a' }}>
            Register New Asset
          </Button>
        )}
      </Box>

      {/* Assets Table */}
      <Card>
        <CardContent sx={{ p: 0 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>
          ) : (
            <Table sx={{ minWidth: 650 }}>
              <TableHead sx={{ bgcolor: '#f8fafc' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>Asset Code</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Asset Name</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Category</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Location</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Condition</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Assigned Vendor</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Next Maintenance</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {assets.map((asset) => (
                  <TableRow key={asset.id} hover>
                    <TableCell sx={{ fontWeight: 700 }}>{asset.assetCode}</TableCell>
                    <TableCell>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>{asset.assetName}</Typography>
                      <Typography variant="caption" color="text.secondary">{asset.manufacturer} - {asset.modelNumber}</Typography>
                    </TableCell>
                    <TableCell><Chip label={asset.categoryName} size="small" variant="outlined" /></TableCell>
                    <TableCell>{asset.towerName || asset.specificLocation || 'Common Area'}</TableCell>
                    <TableCell>
                      <Chip label={asset.condition} color={getConditionColor(asset.condition)} size="small" sx={{ fontWeight: 600 }} />
                    </TableCell>
                    <TableCell>{asset.assignedVendorName || 'In-House'}</TableCell>
                    <TableCell>
                      {asset.nextScheduledMaintenanceDate ? (
                        <Typography variant="body2">
                          {new Date(asset.nextScheduledMaintenanceDate).toLocaleDateString()}
                        </Typography>
                      ) : (
                        '—'
                      )}
                    </TableCell>
                    <TableCell>
                      <Button
                        size="small"
                        startIcon={<BuildIcon />}
                        onClick={() => handleOpenMaintenance(asset)}
                      >
                        Service History
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}

          <Box sx={{ display: 'flex', justifyContent: 'flex-end', p: 2 }}>
            <Pagination count={totalPages} page={page} onChange={(_, v) => setPage(v)} color="primary" />
          </Box>
        </CardContent>
      </Card>

      {/* Service History & Log Maintenance Modal */}
      <Dialog open={openLogModal} onClose={() => setOpenLogModal(false)} maxWidth="md" fullWidth>
        <DialogTitle>Maintenance Records: {selectedAsset?.assetName}</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 3, pt: 1 }}>
          {/* History records */}
          <Box>
            <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1 }}>Past Service Inspections</Typography>
            {maintenanceHistory.length === 0 ? (
              <Typography variant="body2" color="text.secondary">No maintenance entries recorded yet.</Typography>
            ) : (
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Date</TableCell>
                    <TableCell>Technician / Vendor</TableCell>
                    <TableCell>Work Done</TableCell>
                    <TableCell>Cost</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {maintenanceHistory.map((h) => (
                    <TableRow key={h.id}>
                      <TableCell>{new Date(h.maintenanceDate).toLocaleDateString()}</TableCell>
                      <TableCell>{h.performedBy}</TableCell>
                      <TableCell>{h.findingsAndWorkDone}</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>₹{h.costIncurred.toLocaleString('en-IN')}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </Box>

          {canManage && (
            <Box component="form" onSubmit={handleSaveMaintenance} sx={{ display: 'flex', flexDirection: 'column', gap: 2, bgcolor: '#f8fafc', p: 2, borderRadius: 2 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>Log New Maintenance Activity</Typography>
              <TextField
                label="Performed By (Technician / Vendor Name)"
                required
                size="small"
                value={maintenanceForm.performedBy}
                onChange={(e) => setMaintenanceForm({ ...maintenanceForm, performedBy: e.target.value })}
              />
              <TextField
                label="Cost Incurred (₹)"
                type="number"
                size="small"
                value={maintenanceForm.costIncurred}
                onChange={(e) => setMaintenanceForm({ ...maintenanceForm, costIncurred: parseFloat(e.target.value) || 0 })}
              />
              <TextField
                label="Findings & Work Done"
                multiline
                rows={2}
                required
                size="small"
                value={maintenanceForm.findingsAndWorkDone}
                onChange={(e) => setMaintenanceForm({ ...maintenanceForm, findingsAndWorkDone: e.target.value })}
              />
              <TextField
                label="Parts Replaced (if any)"
                size="small"
                value={maintenanceForm.partsReplaced}
                onChange={(e) => setMaintenanceForm({ ...maintenanceForm, partsReplaced: e.target.value })}
              />
              <TextField
                label="Next Recommended Service Date"
                type="date"
                size="small"
                slotProps={{ inputLabel: { shrink: true } }}
                value={maintenanceForm.nextServiceDate}
                onChange={(e) => setMaintenanceForm({ ...maintenanceForm, nextServiceDate: e.target.value })}
              />
              <Button type="submit" variant="contained" sx={{ alignSelf: 'flex-start', bgcolor: '#1e3a8a' }}>
                Save Inspection Record
              </Button>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenLogModal(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Add Asset Modal */}
      <Dialog open={openAddModal} onClose={() => setOpenAddModal(false)} maxWidth="sm" fullWidth>
        <form onSubmit={handleCreateAsset}>
          <DialogTitle>Register New Asset</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            <TextField
              label="Asset Code (e.g. AST-GEN-02)"
              required
              value={newAsset.assetCode}
              onChange={(e) => setNewAsset({ ...newAsset, assetCode: e.target.value })}
            />
            <TextField
              label="Asset Name"
              required
              value={newAsset.assetName}
              onChange={(e) => setNewAsset({ ...newAsset, assetName: e.target.value })}
            />
            <TextField
              select
              label="Category"
              value={newAsset.categoryId}
              onChange={(e) => setNewAsset({ ...newAsset, categoryId: parseInt(e.target.value) })}
            >
              {categories.map((c) => (
                <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>
              ))}
            </TextField>
            <TextField
              label="Manufacturer"
              value={newAsset.manufacturer}
              onChange={(e) => setNewAsset({ ...newAsset, manufacturer: e.target.value })}
            />
            <TextField
              label="Model Number"
              value={newAsset.modelNumber}
              onChange={(e) => setNewAsset({ ...newAsset, modelNumber: e.target.value })}
            />
            <TextField
              label="Serial Number"
              value={newAsset.serialNumber}
              onChange={(e) => setNewAsset({ ...newAsset, serialNumber: e.target.value })}
            />
            <TextField
              label="Purchase Cost (₹)"
              type="number"
              required
              value={newAsset.purchaseCost}
              onChange={(e) => setNewAsset({ ...newAsset, purchaseCost: parseFloat(e.target.value) || 0 })}
            />
            <TextField
              label="Specific Location (e.g. Basement 1 Pump Room)"
              value={newAsset.specificLocation}
              onChange={(e) => setNewAsset({ ...newAsset, specificLocation: e.target.value })}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenAddModal(false)}>Cancel</Button>
            <Button type="submit" variant="contained">Register Asset</Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};
