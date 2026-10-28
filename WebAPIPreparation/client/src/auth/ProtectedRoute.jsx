import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from './useAuth';
/** Shows child pages only to logged-in users. Pass roles={['Admin']} for role-only pages. */
export default function ProtectedRoute({ roles }) {
  const { user, loading } = useAuth();
  const location = useLocation();

  if (loading) return <p className="center">Loading…</p>;

  if (!user) {
    // remember where the user wanted to go, to return there after login
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (roles && !roles.includes(user.role)) {
    return <p className="center">You do not have access to this page.</p>;
  }

  return <Outlet />;
}