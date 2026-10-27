import React, { createContext, useContext, useState, useEffect } from 'react';
import api from '../api/client';
import { User, LoginResponse, ApiResponse } from '../types';

interface AuthContextType {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  hasRole: (role: string) => boolean;
  hasAnyRole: (roles: string[]) => boolean;
  isCommitteeMember: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    const saved = localStorage.getItem('currentUser');
    return saved ? JSON.parse(saved) : null;
  });
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('accessToken'));
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    const checkAuth = async () => {
      const storedToken = localStorage.getItem('accessToken');
      if (storedToken) {
        try {
          const res = await api.get<ApiResponse<User>>('/auth/me');
          if (res.data?.success && res.data.data) {
            setUser(res.data.data);
            localStorage.setItem('currentUser', JSON.stringify(res.data.data));
          }
        } catch {
          // Handled by axios interceptor
        }
      }
      setIsLoading(false);
    };

    checkAuth();
  }, []);

  const login = async (email: string, password: string) => {
    const res = await api.post<ApiResponse<LoginResponse>>('/auth/login', { email, password });
    if (res.data.success && res.data.data) {
      const { accessToken, user: loggedUser } = res.data.data;
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('currentUser', JSON.stringify(loggedUser));
      setToken(accessToken);
      setUser(loggedUser);
    } else {
      throw new Error(res.data.message || 'Login failed.');
    }
  };

  const logout = async () => {
    try {
      await api.post('/auth/logout');
    } catch {
      // Ignore
    } finally {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('currentUser');
      setToken(null);
      setUser(null);
      window.location.href = '/login';
    }
  };

  const hasRole = (role: string): boolean => {
    if (!user || !user.roles) return false;
    return user.roles.includes(role);
  };

  const hasAnyRole = (roles: string[]): boolean => {
    if (!user || !user.roles) return false;
    return roles.some((r) => user.roles.includes(r));
  };

  const isCommitteeMember =
    hasRole('President') || hasRole('Secretary') || hasRole('Treasurer') || hasRole('System Administrator');

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated: !!user && !!token,
        isLoading,
        login,
        logout,
        hasRole,
        hasAnyRole,
        isCommitteeMember,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
