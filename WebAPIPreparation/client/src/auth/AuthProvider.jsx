import { useCallback, useEffect, useMemo, useState } from 'react';
import { api, refreshTokens, setOnSessionExpired, tokenStore, userFromToken } from '../api/client';
import { AuthContext } from './AuthContext';

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null); // { userName, role } or null

  // Start in "loading" ONLY if a refresh token is saved (page was reloaded).
  // Deciding it here, instead of calling setLoading(false) inside the effect,
  // avoids an extra render and fixes the set-state-in-effect error.
  const [loading, setLoading] = useState(() => tokenStore.getRefresh() !== null);

  // Let api/client.js log the user out when a refresh fails
  useEffect(() => {
    setOnSessionExpired(() => setUser(null));
  }, []);

  // On page load: if a refresh token is saved, get a fresh access token
  useEffect(() => {
    if (!loading) return;

    refreshTokens()
      .then((data) => setUser(userFromToken(data.accessToken)))   // setState in a callback: OK
      .catch(() => {
        tokenStore.clear();
        setUser(null);
      })
      .finally(() => setLoading(false));
  }, [loading]);

  const login = useCallback(async (userName, password) => {
    // POST /api/auth/login → { accessToken, refreshToken, expiresAt }
    const { data } = await api.post('/auth/login', { userName, password });
    tokenStore.save(data.accessToken, data.refreshToken);
    setUser(userFromToken(data.accessToken));
  }, []);

  const logout = useCallback(async () => {
    const refreshToken = tokenStore.getRefresh();
    try {
      // POST /api/auth/logout → clears the refresh token in the Users table
      if (refreshToken) await api.post('/auth/logout', { refreshToken });
    } catch {
      // even if the call fails, log out on this side
    } finally {
      tokenStore.clear();
      setUser(null);
    }
  }, []);

  const value = useMemo(
    () => ({ user, loading, login, logout, isAdmin: user?.role === 'Admin' }),
    [user, loading, login, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}