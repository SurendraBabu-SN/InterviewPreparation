import { createContext } from 'react';

// The context object lives in its own file, so AuthProvider.jsx exports only a component.
export const AuthContext = createContext(null);