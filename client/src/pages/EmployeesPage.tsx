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
import SearchIcon from '@mui/icons-material/Search';
import api from '../api/client';
import { Employee, ApiResponse, PagedResult } from '../types';
import { useAuth } from '../context/AuthContext';

export const EmployeesPage: React.FC = () => {
  const { hasAnyRole } = useAuth();
  const canManage = hasAnyRole(['System Administrator', 'Secretary']);

  const [employees, setEmployees] = useState<Employee[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [department, setDepartment] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [openModal, setOpenModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [modalError, setModalError] = useState<string | null>(null);

  const [newEmp, setNewEmp] = useState({
    employeeCode: '',
    firstName: '',
    lastName: '',
    designation: '',
    department: 'Estate Operations',
    mobileNumber: '',
    email: '',
    address: '',
    joiningDate: new Date().toISOString().split('T')[0],
    employmentType: 1, // FullTime
    monthlySalary: 35000,
    emergencyContactName: '',
    emergencyContactNumber: '',
  });

  const fetchEmployees = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();
      params.append('page', page.toString());
      params.append('pageSize', '10');
      if (search) params.append('search', search);
      if (department) params.append('department', department);

      const res = await api.get<ApiResponse<PagedResult<Employee>>>(`/employees?${params.toString()}`);
      if (res.data?.success) {
        setEmployees(res.data.data.items);
        setTotalPages(res.data.data.totalPages);
        setTotalCount(res.data.data.totalCount);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEmployees();
  }, [page, department]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchEmployees();
  };

  const handleCreateEmployee = async (e: React.FormEvent) => {
    e.preventDefault();
    setModalError(null);
    try {
      setSubmitting(true);
      const res = await api.post<ApiResponse<Employee>>('/employees', newEmp);
      if (res.data?.success) {
        setOpenModal(false);
        setNewEmp({
          employeeCode: '',
          firstName: '',
          lastName: '',
          designation: '',
          department: 'Estate Operations',
          mobileNumber: '',
          email: '',
          address: '',
          joiningDate: new Date().toISOString().split('T')[0],
          employmentType: 1,
          monthlySalary: 35000,
          emergencyContactName: '',
          emergencyContactNumber: '',
        });
        fetchEmployees();
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to register employee.';
      setModalError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  const handleToggleStatus = async (emp: Employee) => {
    const nextStatus = emp.status === 'Active' ? 5 : 1; // 5 = Terminated/Inactive, 1 = Active
    try {
      await api.patch(`/employees/${emp.id}/status`, nextStatus);
      fetchEmployees();
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 700, color: '#0f172a' }}>
            Employee Management ({totalCount} staff)
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Maintenance supervisors, technicians, security liaisons, and facility staff directory.
          </Typography>
        </Box>
        {canManage && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpenModal(true)} sx={{ bgcolor: '#1e3a8a' }}>
            Add New Employee
          </Button>
        )}
      </Box>

      {/* Filter / Search Bar */}
      <Card sx={{ p: 2 }}>
        <Box component="form" onSubmit={handleSearch} sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
          <TextField
            size="small"
            placeholder="Search code, name, phone..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            sx={{ flexGrow: 1, minWidth: 200 }}
          />
          <TextField
            select
            size="small"
            label="Department"
            value={department}
            onChange={(e) => { setDepartment(e.target.value); setPage(1); }}
            sx={{ minWidth: 200 }}
          >
            <MenuItem value="">All Departments</MenuItem>
            <MenuItem value="Estate Operations">Estate Operations</MenuItem>
            <MenuItem value="Engineering & Maintenance">Engineering & Maintenance</MenuItem>
            <MenuItem value="Housekeeping & Hygiene">Housekeeping & Hygiene</MenuItem>
          </TextField>
          <Button type="submit" variant="outlined" startIcon={<SearchIcon />}>
            Search
          </Button>
        </Box>
      </Card>

      {/* Table */}
      <Card>
        <CardContent sx={{ p: 0 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 5 }}><CircularProgress /></Box>
          ) : (
            <Table sx={{ minWidth: 650 }}>
              <TableHead sx={{ bgcolor: '#f8fafc' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>Code</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Full Name</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Designation</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Department</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Mobile & Email</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Monthly Salary</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Status</TableCell>
                  {canManage && <TableCell sx={{ fontWeight: 700 }}>Action</TableCell>}
                </TableRow>
              </TableHead>
              <TableBody>
                {employees.map((emp) => (
                  <TableRow key={emp.id} hover>
                    <TableCell sx={{ fontWeight: 600 }}>{emp.employeeCode}</TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>{emp.fullName}</TableCell>
                    <TableCell>{emp.designation}</TableCell>
                    <TableCell><Chip label={emp.department} size="small" variant="outlined" /></TableCell>
                    <TableCell>
                      <Typography variant="body2">{emp.mobileNumber}</Typography>
                      <Typography variant="caption" color="text.secondary">{emp.email}</Typography>
                    </TableCell>
                    <TableCell>
                      {emp.monthlySalary !== null && emp.monthlySalary !== undefined ? (
                        <Typography variant="body2" sx={{ fontWeight: 600, color: '#16a34a' }}>
                          ₹{emp.monthlySalary.toLocaleString('en-IN')}
                        </Typography>
                      ) : (
                        <Typography variant="caption" color="text.secondary">Restricted</Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Chip label={emp.status} color={emp.status === 'Active' ? 'success' : 'default'} size="small" />
                    </TableCell>
                    {canManage && (
                      <TableCell>
                        <Button size="small" onClick={() => handleToggleStatus(emp)}>
                          {emp.status === 'Active' ? 'Deactivate' : 'Activate'}
                        </Button>
                      </TableCell>
                    )}
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

      {/* Add Employee Modal */}
      <Dialog open={openModal} onClose={() => setOpenModal(false)} maxWidth="sm" fullWidth>
        <form onSubmit={handleCreateEmployee}>
          <DialogTitle>Register New Employee</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            {modalError && <Alert severity="error">{modalError}</Alert>}
            <TextField
              label="Employee Code (e.g. EMP-004)"
              required
              value={newEmp.employeeCode}
              onChange={(e) => setNewEmp({ ...newEmp, employeeCode: e.target.value })}
            />
            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                label="First Name"
                required
                fullWidth
                value={newEmp.firstName}
                onChange={(e) => setNewEmp({ ...newEmp, firstName: e.target.value })}
              />
              <TextField
                label="Last Name"
                required
                fullWidth
                value={newEmp.lastName}
                onChange={(e) => setNewEmp({ ...newEmp, lastName: e.target.value })}
              />
            </Box>
            <TextField
              label="Designation (e.g. Senior Plumber)"
              required
              value={newEmp.designation}
              onChange={(e) => setNewEmp({ ...newEmp, designation: e.target.value })}
            />
            <TextField
              select
              label="Department"
              value={newEmp.department}
              onChange={(e) => setNewEmp({ ...newEmp, department: e.target.value })}
            >
              <MenuItem value="Estate Operations">Estate Operations</MenuItem>
              <MenuItem value="Engineering & Maintenance">Engineering & Maintenance</MenuItem>
              <MenuItem value="Housekeeping & Hygiene">Housekeeping & Hygiene</MenuItem>
            </TextField>
            <TextField
              label="Mobile Number"
              required
              value={newEmp.mobileNumber}
              onChange={(e) => setNewEmp({ ...newEmp, mobileNumber: e.target.value })}
            />
            <TextField
              label="Email Address"
              required
              type="email"
              value={newEmp.email}
              onChange={(e) => setNewEmp({ ...newEmp, email: e.target.value })}
            />
            <TextField
              label="Monthly Compensation (₹)"
              type="number"
              required
              value={newEmp.monthlySalary}
              onChange={(e) => setNewEmp({ ...newEmp, monthlySalary: parseFloat(e.target.value) || 0 })}
            />
            <TextField
              label="Residential Address"
              multiline
              rows={2}
              value={newEmp.address}
              onChange={(e) => setNewEmp({ ...newEmp, address: e.target.value })}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setOpenModal(false)}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={submitting}>
              {submitting ? 'Registering...' : 'Add Employee'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </Box>
  );
};
