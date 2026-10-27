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
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong';
import VisibilityIcon from '@mui/icons-material/Visibility';
import api from '../api/client';
import { PurchaseOrder, ApiResponse } from '../types';

export const PurchaseOrdersPage: React.FC = () => {
  const [orders, setOrders] = useState<PurchaseOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedPO, setSelectedPO] = useState<PurchaseOrder | null>(null);
  const [detailsModalOpen, setDetailsModalOpen] = useState(false);

  const fetchOrders = async () => {
    try {
      setLoading(true);
      const res = await api.get<ApiResponse<PurchaseOrder[]>>('/purchase-orders');
      if (res.data?.success) {
        setOrders(res.data.data);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrders();
  }, []);

  const handleOpenDetails = (po: PurchaseOrder) => {
    setSelectedPO(po);
    setDetailsModalOpen(true);
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
          Purchase Orders ({orders.length} orders)
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Legally binding orders generated after three-tier committee approval sign-off.
        </Typography>
      </Box>

      {/* Orders Table */}
      <Card>
        <CardContent sx={{ p: 0 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>
          ) : orders.length === 0 ? (
            <Box sx={{ p: 5, textAlign: 'center' }}>
              <ReceiptLongIcon sx={{ fontSize: 48, color: '#94a3b8', mb: 1.5 }} />
              <Typography variant="h6" color="text.secondary">No Purchase Orders yet</Typography>
              <Typography variant="caption" color="text.secondary">
                Purchase orders are generated from approved purchase requests.
              </Typography>
            </Box>
          ) : (
            <Table sx={{ minWidth: 650 }}>
              <TableHead sx={{ bgcolor: '#f8fafc' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>PO Number</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Original PR</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Vendor / Contractor</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Order Date</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Total Value</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Delivery Status</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Action</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {orders.map((po) => (
                  <TableRow key={po.id} hover>
                    <TableCell sx={{ fontWeight: 700, color: '#1e3a8a' }}>{po.poNumber}</TableCell>
                    <TableCell>{po.purchaseRequestNumber}</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>{po.vendorName}</TableCell>
                    <TableCell>{new Date(po.orderDateUtc).toLocaleDateString()}</TableCell>
                    <TableCell sx={{ fontWeight: 700, color: '#16a34a' }}>
                      ₹{po.grandTotal.toLocaleString('en-IN')}
                    </TableCell>
                    <TableCell>
                      <Chip label={po.deliveryStatus} color="primary" size="small" sx={{ fontWeight: 600 }} />
                    </TableCell>
                    <TableCell>
                      <Button
                        size="small"
                        startIcon={<VisibilityIcon />}
                        onClick={() => handleOpenDetails(po)}
                      >
                        Details
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Details Modal */}
      <Dialog open={detailsModalOpen} onClose={() => setDetailsModalOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Purchase Order: {selectedPO?.poNumber}</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
          <Box sx={{ bgcolor: '#f8fafc', p: 2, borderRadius: 2 }}>
            <Typography variant="body2">Vendor: <strong>{selectedPO?.vendorName}</strong></Typography>
            <Typography variant="body2">Associated PR: <strong>{selectedPO?.purchaseRequestNumber}</strong></Typography>
            <Typography variant="body2">Order Date: <strong>{selectedPO?.orderDateUtc ? new Date(selectedPO.orderDateUtc).toLocaleDateString() : ''}</strong></Typography>
            <Typography variant="body2" sx={{ fontWeight: 700, color: '#16a34a', mt: 1 }}>
              Grand Total: ₹{selectedPO?.grandTotal.toLocaleString('en-IN')}
            </Typography>
          </Box>

          <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>Ordered Items</Typography>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Description</TableCell>
                <TableCell>Qty</TableCell>
                <TableCell>Unit Price</TableCell>
                <TableCell>Total</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {selectedPO?.items.map((item) => (
                <TableRow key={item.id}>
                  <TableCell>{item.itemDescription}</TableCell>
                  <TableCell>{item.quantity} {item.unitOfMeasure}</TableCell>
                  <TableCell>₹{item.unitPrice.toLocaleString('en-IN')}</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>₹{item.totalPrice.toLocaleString('en-IN')}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailsModalOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};
