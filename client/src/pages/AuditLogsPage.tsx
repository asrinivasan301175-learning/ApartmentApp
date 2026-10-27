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
  TextField,
  Pagination,
  CircularProgress,
  Button,
} from '@mui/material';
import SearchIcon from '@mui/icons-material/Search';
import SecurityIcon from '@mui/icons-material/Security';
import api from '../api/client';
import { AuditLog, ApiResponse, PagedResult } from '../types';

export const AuditLogsPage: React.FC = () => {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [entityFilter, setEntityFilter] = useState('');
  const [actionFilter, setActionFilter] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const fetchLogs = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', '15');
      if (entityFilter) params.append('entityName', entityFilter);
      if (actionFilter) params.append('action', actionFilter);

      const res = await api.get<ApiResponse<PagedResult<AuditLog>>>(`/auditlogs?${params.toString()}`);
      if (res.data?.success) {
        setLogs(res.data.data.items);
        setTotalPages(res.data.data.totalPages);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLogs();
  }, [page]);

  const handleFilter = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchLogs();
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box>
        <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
          System Audit Trail & Compliance Log
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Immutable audit record of all administrative, financial, and approval actions across the platform.
        </Typography>
      </Box>

      {/* Filter Bar */}
      <Card sx={{ p: 2 }}>
        <Box component="form" onSubmit={handleFilter} sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
          <TextField
            size="small"
            placeholder="Entity Name (e.g. PurchaseRequest, Flat)..."
            value={entityFilter}
            onChange={(e) => setEntityFilter(e.target.value)}
            sx={{ minWidth: 240 }}
          />
          <TextField
            size="small"
            placeholder="Action (e.g. Create, Approval-Approved)..."
            value={actionFilter}
            onChange={(e) => setActionFilter(e.target.value)}
            sx={{ minWidth: 240 }}
          />
          <Button type="submit" variant="outlined" startIcon={<SearchIcon />}>
            Filter Logs
          </Button>
        </Box>
      </Card>

      {/* Audit Table */}
      <Card>
        <CardContent sx={{ p: 0 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>
          ) : logs.length === 0 ? (
            <Box sx={{ p: 5, textAlign: 'center' }}>
              <SecurityIcon sx={{ fontSize: 48, color: '#94a3b8', mb: 1.5 }} />
              <Typography variant="h6" color="text.secondary">No matching audit logs found.</Typography>
            </Box>
          ) : (
            <Table sx={{ minWidth: 650 }}>
              <TableHead sx={{ bgcolor: '#f8fafc' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>Timestamp (UTC)</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Action</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Entity</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Entity ID</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Acting User</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Details</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {logs.map((log) => (
                  <TableRow key={log.id} hover>
                    <TableCell sx={{ whiteSpace: 'nowrap' }}>
                      {new Date(log.timestampUtc).toLocaleString()}
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={log.action}
                        size="small"
                        color={log.action.includes('Approve') ? 'success' : log.action.includes('Reject') ? 'error' : 'primary'}
                      />
                    </TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>{log.entityName}</TableCell>
                    <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>
                      {log.entityId}
                    </TableCell>
                    <TableCell>{log.userEmail || 'System / Batch'}</TableCell>
                    <TableCell sx={{ maxWidth: 350, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {log.newValues || '—'}
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
    </Box>
  );
};
