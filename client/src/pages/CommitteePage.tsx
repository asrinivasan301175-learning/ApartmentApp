import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  Chip,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  CircularProgress,
  Alert,
} from '@mui/material';
import HowToVoteIcon from '@mui/icons-material/HowToVote';
import PersonAddIcon from '@mui/icons-material/PersonAdd';
import api from '../api/client';
import { ApiResponse, User } from '../types';
import { useAuth } from '../context/AuthContext';

interface CommitteeMemberItem {
  id: number;
  userId: string;
  memberName: string;
  memberEmail: string;
  memberPhone: string;
  committeeRole: string;
  termStartDateUtc: string;
  termEndDateUtc: string;
  isCurrentActive: boolean;
  remarks?: string;
}

export const CommitteePage: React.FC = () => {
  const { hasRole } = useAuth();
  const isAdmin = hasRole('System Administrator');

  const [members, setMembers] = useState<CommitteeMemberItem[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [openAssignModal, setOpenAssignModal] = useState(false);
  const [assignForm, setAssignForm] = useState({
    userId: '',
    committeeRole: 'President',
    termStartDateUtc: new Date().toISOString().split('T')[0],
    termEndDateUtc: new Date(Date.now() + 730 * 86400000).toISOString().split('T')[0], // 2 years
    remarks: 'Elected committee member',
  });
  const [error, setError] = useState<string | null>(null);

  const fetchMembers = async () => {
    try {
      setLoading(true);
      const res = await api.get<ApiResponse<CommitteeMemberItem[]>>('/committee?activeOnly=false');
      if (res.data?.success) {
        setMembers(res.data.data);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchUsers = async () => {
    try {
      const res = await api.get<ApiResponse<{ items: User[] }>>('/users?pageSize=100');
      if (res.data?.success) {
        setUsers(res.data.data.items);
        if (res.data.data.items.length > 0) {
          setAssignForm((prev) => ({ ...prev, userId: res.data.data.items[0].id }));
        }
      }
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchMembers();
    if (isAdmin) fetchUsers();
  }, [isAdmin]);

  const handleAssign = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      const res = await api.post('/committee/assign', assignForm);
      if (res.data?.success) {
        setOpenAssignModal(false);
        fetchMembers();
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to assign committee position.';
      setError(msg);
    }
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
            Executive Management Committee
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Elected committee leadership holding signing and purchase order approval authority.
          </Typography>
        </Box>
        {isAdmin && (
          <Button
            variant="contained"
            startIcon={<PersonAddIcon />}
            onClick={() => setOpenAssignModal(true)}
            sx={{ bgcolor: '#1e3a8a' }}
          >
            Assign Committee Role
          </Button>
        )}
      </Box>

      {/* Committee Members Cards */}
      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>
      ) : (
        <Grid container spacing={3}>
          {members.map((m) => (
            <Grid size={{ xs: 12, md: 4 }} key={m.id}>
              <Card sx={{ height: '100%', borderTop: `4px solid ${m.committeeRole === 'President' ? '#7c3aed' : m.committeeRole === 'Secretary' ? '#0284c7' : '#059669'}` }}>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                    <Chip
                      label={m.committeeRole}
                      color={m.committeeRole === 'President' ? 'secondary' : m.committeeRole === 'Secretary' ? 'info' : 'success'}
                      sx={{ fontWeight: 700 }}
                    />
                    <Chip
                      label={m.isCurrentActive ? 'Active Tenure' : 'Past Member'}
                      size="small"
                      color={m.isCurrentActive ? 'success' : 'default'}
                    />
                  </Box>
                  <Typography variant="h6" sx={{ fontWeight: 700 }}>
                    {m.memberName}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
                    {m.memberEmail} • {m.memberPhone}
                  </Typography>
                  <Box sx={{ bgcolor: '#f8fafc', p: 1.5, borderRadius: 1.5 }}>
                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                      Tenure Period:
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {new Date(m.termStartDateUtc).toLocaleDateString()} to {new Date(m.termEndDateUtc).toLocaleDateString()}
                    </Typography>
                    {m.remarks && (
                      <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                        {m.remarks}
                      </Typography>
                    )}
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {/* Assign Modal */}
      <Dialog open={openAssignModal} onClose={() => setOpenAssignModal(false)} maxWidth="sm" fullWidth>
        <form onSubmit={handleAssign}>
          <DialogTitle>Assign Committee Office</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField
              select
              label="Select User Account"
              required
              value={assignForm.userId}
              onChange={(e) => setAssignForm({ ...assignForm, userId: e.target.value })}
            >
              {users.map((u) => (
                <MenuItem key={u.id} value={u.id}>
                  {u.fullName} ({u.email})
                </MenuItem>
              ))}
            </TextField>
            <TextField
              select
              label="Committee Position"
              value={assignForm.committeeRole}
              onChange={(e) => setAssignForm({ ...assignForm, committeeRole: e.target.value })}
            >
              <MenuItem value="President">President</MenuItem>
              <MenuItem value="Secretary">Secretary</MenuItem>
              <MenuItem value="Treasurer">Treasurer</MenuItem>
            </TextField>
            <TextField
              label="Term Start Date"
              type="date"
              slotProps={{ inputLabel: { shrink: true } }}
              value={assignForm.termStartDateUtc}
              onChange={(e) => setAssignForm({ ...assignForm, termStartDateUtc: e.target.value })}
            />
            <TextField
              label="Term End Date"
              type="date"
              slotProps={{ inputLabel: { shrink: true } }}
              value={assignForm.termEndDateUtc}
              onChange={(e) => setAssignForm({ ...assignForm, termEndDateUtc: e.target.value })}
            />
            <TextField
              label="Remarks / Resolution Ref"
              value={assignForm.remarks}
              onChange={(e) => setAssignForm({ ...assignForm, remarks: e.target.value })}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenAssignModal(false)}>Cancel</Button>
            <Button type="submit" variant="contained">Confirm Assignment</Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};
