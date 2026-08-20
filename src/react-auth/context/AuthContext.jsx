import { createContext, useContext, useEffect, useMemo, useState } from 'react';

const AuthContext = createContext(null);
const TOKEN_KEY = 'token';

const decodeJwt = (token) => {
  if (!token) return null;

  try {
    const payload = token.split('.')[1];
    if (!payload) return null;

    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
    const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=');
    const decoded = atob(padded);

    return JSON.parse(decodeURIComponent(
      Array.from(decoded).map((char) => {
        const escape = `%%%${char.charCodeAt(0).toString(16).toUpperCase()}`;
        return escape.length === 3 ? escape : `%${escape}`;
      }).join('')
    ));
  } catch (error) {
    console.error('Failed to decode JWT:', error);
    return null;
  }
};

const isTokenExpired = (token) => {
  const decoded = decodeJwt(token);
  if (!decoded || !decoded.exp) return true;

  return Date.now() >= decoded.exp * 1000;
};

const getRoles = (claims) => {
  const roleClaim = claims?.role
    || claims?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  return Array.isArray(roleClaim) ? roleClaim : roleClaim ? [roleClaim] : [];
};

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem(TOKEN_KEY) || null);
  const [user, setUser] = useState(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const initializeAuth = () => {
      const savedToken = localStorage.getItem(TOKEN_KEY);

      if (!savedToken || isTokenExpired(savedToken)) {
        localStorage.removeItem(TOKEN_KEY);
        setToken(null);
        setUser(null);
        setIsAuthenticated(false);
        setIsLoading(false);
        return;
      }

      const decodedUser = decodeJwt(savedToken);
      setUser(decodedUser);
      setToken(savedToken);
      setIsAuthenticated(true);
      setIsLoading(false);
    };

    initializeAuth();
  }, []);

  const login = (jwtToken) => {
    if (!jwtToken) {
      throw new Error('A JWT token is required to login.');
    }

    const decodedUser = decodeJwt(jwtToken);

    if (!decodedUser || isTokenExpired(jwtToken)) {
      throw new Error('The provided token is invalid or expired.');
    }

    localStorage.setItem(TOKEN_KEY, jwtToken);
    setToken(jwtToken);
    setUser(decodedUser);
    setIsAuthenticated(true);
  };

  const logout = () => {
    localStorage.removeItem(TOKEN_KEY);
    setToken(null);
    setUser(null);
    setIsAuthenticated(false);
  };

  useEffect(() => {
    if (!token) {
      setIsAuthenticated(false);
      setUser(null);
      return;
    }

    const decodedUser = decodeJwt(token);

    if (!decodedUser || isTokenExpired(token)) {
      logout();
      return;
    }

    setUser(decodedUser);
    setIsAuthenticated(true);
  }, [token]);

  const value = useMemo(
    () => ({
      token,
      user,
      isAuthenticated,
      isLoading,
      roles: getRoles(user),
      isAdmin: getRoles(user).includes('Admin'),
      login,
      logout,
    }),
    [token, user, isAuthenticated, isLoading]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export const useAuth = () => {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.');
  }

  return context;
};

export default AuthContext;
