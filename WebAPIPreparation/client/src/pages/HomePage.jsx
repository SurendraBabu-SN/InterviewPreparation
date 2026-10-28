import { useEffect, useState } from 'react';
import { api, getErrorMessage } from '../api/client';
import { useAuth } from '../auth/useAuth';

export default function HomePage() {
  const { user } = useAuth();
  const [countries, setCountries] = useState([]);
  const [error, setError] = useState('');

  useEffect(() => {
    api.get('/Country')
      .then(({ data }) => setCountries(data))
      .catch((err) => setError(getErrorMessage(err)));
  }, []);

  return (
    <section>
      <h2>Welcome, {user.userName}</h2>
      <p className="muted">Your role: {user.role}</p>

      <h3>Countries (loaded with your token)</h3>
      {error && <p className="error">{error}</p>}
      <ul>
        {countries.map((c) => <li key={c.id}>{c.countryName}</li>)}
      </ul>
      {!error && countries.length === 0 && <p className="muted">No countries yet.</p>}
    </section>
  );
}