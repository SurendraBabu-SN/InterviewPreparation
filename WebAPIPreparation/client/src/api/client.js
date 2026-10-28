import axios from 'axios';

// Access token: in memory only (safest; lost on page reload)
// Refresh token: sessionStorage (survives page reload, cleared when the tab closes)
const REFRESH_KEY = 'refreshToken';
let accessToken = null;

export const tokenStore = {
  getAccess: () => accessToken,
  getRefresh: () => sessionStorage.getItem(REFRESH_KEY),
  save(access, refresh) {
    accessToken = access;
    if (refresh) sessionStorage.setItem(REFRESH_KEY, refresh);
  },
  clear() {
    accessToken = null;
    sessionStorage.removeItem(REFRESH_KEY);
  }
};

// AuthContext registers a function here, so this file can log the user out.
let onSessionExpired = () => {};
export const setOnSessionExpired = (fn) => { onSessionExpired = fn; };

// All API calls go through this. "/api" is forwarded to .NET by the Vite proxy.
export const api = axios.create({ baseURL: '/api' });

// 1. Add "Authorization: Bearer <access token>" to every request
api.interceptors.request.use((config) => {
  if (accessToken) config.headers.Authorization = `Bearer ${accessToken}`;
  return config;
});

// 2. Get new tokens using the refresh token.
//    Several failing calls share ONE refresh call, because the API rotates
//    refresh tokens: a second refresh with the same old token would fail.
let refreshPromise = null;

export function refreshTokens() {
  const refreshToken = tokenStore.getRefresh();
  if (!refreshToken) return Promise.reject(new Error('No refresh token'));

  refreshPromise ??= axios
    .post('/api/auth/refresh-token', { refreshToken })
    .then(({ data }) => {
      tokenStore.save(data.accessToken, data.refreshToken);
      return data;
    })
    .finally(() => { refreshPromise = null; });

  return refreshPromise;
}

// 3. On 401 (access token expired): refresh once, then retry the call
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;
    const isAuthCall = original?.url?.startsWith('/auth/');

    if (error.response?.status === 401 && original && !original._retry && !isAuthCall) {
      original._retry = true;
      try {
        await refreshTokens();
        return api(original);
      } catch {
        tokenStore.clear();
        onSessionExpired(); // app goes back to the login page
      }
    }
    return Promise.reject(error);
  }
);

// Read user name and role from the access token (JWT payload).
// .NET's ClaimTypes.Name / ClaimTypes.Role use these long claim names.
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

export function userFromToken(token) {
  try {
    const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const payload = JSON.parse(atob(base64));
    return {
      userName: payload[NAME_CLAIM] ?? payload.name ?? '',
      role: payload[ROLE_CLAIM] ?? payload.role ?? ''
    };
  } catch {
    return null;
  }
}

/** Turns an API error into a short message for the screen. */
export function getErrorMessage(error) {
  const status = error?.response?.status;
  const data = error?.response?.data;

  if (!error?.response) return 'Cannot reach the server. Is the API running?';
  if (data?.message) return data.message;
  if (data?.errors) return Object.values(data.errors).flat().join(' ');
  if (data?.title) return data.title;
  if (status === 401) return 'Wrong user name or password.';
  if (status === 403) return 'You do not have permission to do this.';
  return 'Something went wrong. Please try again.';
}