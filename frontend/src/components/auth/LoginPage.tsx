import { useState } from 'react';
import { api, setApiKey, setAuthToken } from '../../api/client';
import type { AuthResponse } from '../../types';

interface Props {
  onAuthenticated: () => void;
}

type Mode = 'login' | 'register' | 'apikey';

export function LoginPage({ onAuthenticated }: Props) {
  const [mode, setMode] = useState<Mode>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [name, setName] = useState('');
  const [apiKeyInput, setApiKeyInput] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async () => {
    setError('');
    setLoading(true);
    try {
      const res = await api.post<AuthResponse>('/auth/login', { email, password });
      setAuthToken(res.token);
      setApiKey(res.api_key);
      onAuthenticated();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed');
    } finally {
      setLoading(false);
    }
  };

  const handleRegister = async () => {
    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }
    setError('');
    setLoading(true);
    try {
      const res = await api.post<AuthResponse>('/auth/register', { name, email, password });
      setAuthToken(res.token);
      setApiKey(res.api_key);
      onAuthenticated();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Registration failed');
    } finally {
      setLoading(false);
    }
  };

  const handleApiKey = () => {
    if (apiKeyInput.trim()) {
      setApiKey(apiKeyInput.trim());
      onAuthenticated();
    }
  };

  const inputStyle: React.CSSProperties = {
    padding: '10px 16px', width: '100%', background: 'var(--color-bg-input)',
    border: '1px solid var(--color-border-secondary)', borderRadius: 6,
    color: 'var(--color-text-bright)', fontSize: 14, outline: 'none', boxSizing: 'border-box',
    marginBottom: 12,
  };

  const buttonStyle: React.CSSProperties = {
    padding: '10px 32px', background: 'var(--color-accent)', color: '#fff',
    border: 'none', borderRadius: 6, cursor: 'pointer', fontSize: 14,
    width: '100%', opacity: loading ? 0.7 : 1,
  };

  const linkStyle: React.CSSProperties = {
    background: 'none', border: 'none', color: 'var(--color-accent)',
    cursor: 'pointer', fontSize: 13, padding: 0,
  };

  return (
    <div style={{
      display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh',
      background: 'var(--color-bg-primary)', color: 'var(--color-text-bright)',
    }}>
      <div style={{ textAlign: 'center', width: 360 }}>
        <h1 style={{ marginBottom: 8, fontSize: 28, fontWeight: 600 }}>Restward</h1>
        <p style={{ color: 'var(--color-text-muted)', marginBottom: 24, fontSize: 14 }}>
          {mode === 'login' ? 'Sign in to your account' :
           mode === 'register' ? 'Create a new account' :
           'Enter your API key'}
        </p>

        {error && (
          <p style={{ color: 'var(--color-error)', fontSize: 13, marginBottom: 12 }}>{error}</p>
        )}

        {mode === 'login' && (
          <>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Email"
              style={inputStyle}
              onKeyDown={(e) => e.key === 'Enter' && handleLogin()}
            />
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Password"
              style={inputStyle}
              onKeyDown={(e) => e.key === 'Enter' && handleLogin()}
            />
            <button onClick={handleLogin} disabled={loading} style={buttonStyle}>
              {loading ? 'Signing in...' : 'Sign In'}
            </button>
            <div style={{ marginTop: 16, display: 'flex', justifyContent: 'center', gap: 16 }}>
              <button onClick={() => { setMode('register'); setError(''); }} style={linkStyle}>
                Create account
              </button>
              <button onClick={() => { setMode('apikey'); setError(''); }} style={linkStyle}>
                Use API Key
              </button>
            </div>
          </>
        )}

        {mode === 'register' && (
          <>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Name"
              style={inputStyle}
            />
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Email"
              style={inputStyle}
            />
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Password"
              style={inputStyle}
            />
            <input
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder="Confirm Password"
              style={inputStyle}
              onKeyDown={(e) => e.key === 'Enter' && handleRegister()}
            />
            <button onClick={handleRegister} disabled={loading} style={buttonStyle}>
              {loading ? 'Creating account...' : 'Create Account'}
            </button>
            <div style={{ marginTop: 16 }}>
              <button onClick={() => { setMode('login'); setError(''); }} style={linkStyle}>
                Back to sign in
              </button>
            </div>
          </>
        )}

        {mode === 'apikey' && (
          <>
            <input
              type="password"
              value={apiKeyInput}
              onChange={(e) => setApiKeyInput(e.target.value)}
              placeholder="API Key"
              style={inputStyle}
              onKeyDown={(e) => e.key === 'Enter' && handleApiKey()}
            />
            <button onClick={handleApiKey} style={buttonStyle}>
              Connect
            </button>
            <div style={{ marginTop: 16 }}>
              <button onClick={() => { setMode('login'); setError(''); }} style={linkStyle}>
                Back to sign in
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
