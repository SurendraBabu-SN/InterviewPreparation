import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';

export default function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login', { replace: true });
  };

  return (
    <>
      <header className="topbar">
        <strong>Interview Preparation</strong>
        <nav>
          <NavLink to="/" end>Home</NavLink>
          {/* Later: Countries, States, Customers, Users (Admin) */}
        </nav>
        <span className="who">
          {user.userName} ({user.role})
          <button className="link" onClick={handleLogout}>Logout</button>
        </span>
      </header>

      <main className="page">
        <Outlet />
      </main>
    </>
  );
}