import { useState } from 'react';

interface Props {
  onClose: () => void;
}

type HelpTab = 'getting-started' | 'collections' | 'environments' | 'teams' | 'auth' | 'shortcuts';

const tabs: { id: HelpTab; label: string }[] = [
  { id: 'getting-started', label: 'Getting Started' },
  { id: 'collections', label: 'Collections' },
  { id: 'environments', label: 'Environments' },
  { id: 'teams', label: 'Teams' },
  { id: 'auth', label: 'Authentication' },
  { id: 'shortcuts', label: 'Shortcuts' },
];

function GettingStarted() {
  return (
    <div>
      <h3 style={{ margin: '0 0 12px', fontSize: 16 }}>Getting Started with Restward</h3>
      <p style={{ margin: '0 0 12px', lineHeight: 1.6 }}>
        Restward is a REST API client for building, testing, and organizing HTTP requests.
      </p>
      <ol style={{ margin: 0, paddingLeft: 20, lineHeight: 2 }}>
        <li><strong>Create a collection</strong> — Click the <strong>+</strong> button in the sidebar to organize your requests.</li>
        <li><strong>Add a request</strong> — Select a collection, then click <strong>+ Request</strong> to create a new one.</li>
        <li><strong>Configure the request</strong> — Set the HTTP method, URL, headers, query parameters, and body.</li>
        <li><strong>Send it</strong> — Click <strong>Send</strong> or press <strong>Enter</strong> in the URL bar to execute the request.</li>
        <li><strong>View the response</strong> — Status, headers, and body appear in the response panel below.</li>
      </ol>
    </div>
  );
}

function CollectionsHelp() {
  return (
    <div>
      <h3 style={{ margin: '0 0 12px', fontSize: 16 }}>Collections &amp; Requests</h3>
      <p style={{ margin: '0 0 12px', lineHeight: 1.6 }}>
        Collections let you organize related requests into groups.
      </p>
      <h4 style={{ margin: '16px 0 8px', fontSize: 14 }}>Organizing Requests</h4>
      <ul style={{ margin: 0, paddingLeft: 20, lineHeight: 1.8 }}>
        <li>Create collections for different projects or API services.</li>
        <li>Use folders within collections to further group requests.</li>
        <li>Click any saved request in the sidebar to load it into the editor.</li>
      </ul>
      <h4 style={{ margin: '16px 0 8px', fontSize: 14 }}>Saving</h4>
      <ul style={{ margin: 0, paddingLeft: 20, lineHeight: 1.8 }}>
        <li>Use the <strong>Save</strong> option to save a request to a collection.</li>
        <li>Saved requests auto-save as you edit — look for the "Saved" indicator near the Send button.</li>
      </ul>
      <h4 style={{ margin: '16px 0 8px', fontSize: 14 }}>Import &amp; Export</h4>
      <ul style={{ margin: 0, paddingLeft: 20, lineHeight: 1.8 }}>
        <li>Export collections as JSON for backup or sharing.</li>
        <li>Import Restward JSON files to restore collections.</li>
      </ul>
    </div>
  );
}

function EnvironmentsHelp() {
  return (
    <div>
      <h3 style={{ margin: '0 0 12px', fontSize: 16 }}>Environments &amp; Variables</h3>
      <p style={{ margin: '0 0 12px', lineHeight: 1.6 }}>
        Environments let you define variables that are substituted into your requests,
        making it easy to switch between different servers or configurations.
      </p>

      <h4 style={{ margin: '16px 0 8px', fontSize: 14 }}>Creating an Environment</h4>
      <ol style={{ margin: 0, paddingLeft: 20, lineHeight: 1.8 }}>
        <li>Click the <strong>Environments</strong> button in the top bar.</li>
        <li>Type a name (e.g., "Development") and click <strong>+</strong> to create it.</li>
        <li>Add variables as key/value pairs. Use the checkbox to enable or disable each variable.</li>
        <li>Changes auto-save as you edit.</li>
      </ol>

      <h4 style={{ margin: '16px 0 8px', fontSize: 14 }}>Using Variables</h4>
      <p style={{ margin: '0 0 8px', lineHeight: 1.6 }}>
        Use the <code style={{ background: 'var(--color-bg-elevated)', padding: '2px 6px', borderRadius: 3, fontSize: 13 }}>{'{{variableName}}'}</code> syntax
        anywhere in your requests — URLs, headers, query parameters, and request bodies.
      </p>
      <div style={{
        background: 'var(--color-bg-elevated)', padding: 12, borderRadius: 6, marginBottom: 12,
        border: '1px solid var(--color-border-secondary)', fontSize: 13, lineHeight: 1.8,
      }}>
        <div><strong>Example:</strong></div>
        <div style={{ marginTop: 8 }}>
          <span style={{ color: 'var(--color-text-muted)' }}>1. Define a variable:</span><br />
          &nbsp;&nbsp;<code>baseUrl</code> = <code>https://api.example.com</code>
        </div>
        <div style={{ marginTop: 8 }}>
          <span style={{ color: 'var(--color-text-muted)' }}>2. Use it in your request URL:</span><br />
          &nbsp;&nbsp;<code>{'{{baseUrl}}/users'}</code>
        </div>
        <div style={{ marginTop: 8 }}>
          <span style={{ color: 'var(--color-text-muted)' }}>3. Resolves to:</span><br />
          &nbsp;&nbsp;<code>https://api.example.com/users</code>
        </div>
      </div>

      <h4 style={{ margin: '16px 0 8px', fontSize: 14 }}>Selecting an Active Environment</h4>
      <p style={{ margin: '0 0 8px', lineHeight: 1.6 }}>
        Use the environment dropdown in the top bar to select which environment is active.
        Only enabled variables from the active environment are substituted when sending requests.
      </p>

      <h4 style={{ margin: '16px 0 8px', fontSize: 14 }}>Common Variable Examples</h4>
      <div style={{
        background: 'var(--color-bg-elevated)', padding: 12, borderRadius: 6,
        border: '1px solid var(--color-border-secondary)', fontSize: 13, lineHeight: 1.8,
      }}>
        <div><code>baseUrl</code> = <code>https://api.example.com</code></div>
        <div><code>authToken</code> = <code>Bearer eyJhbGci...</code></div>
        <div><code>apiVersion</code> = <code>v2</code></div>
        <div style={{ marginTop: 8, color: 'var(--color-text-muted)' }}>
          URL: <code>{'{{baseUrl}}/{{apiVersion}}/users'}</code><br />
          Header: <code>Authorization: {'{{authToken}}'}</code>
        </div>
      </div>
    </div>
  );
}

function TeamsHelp() {
  return (
    <div>
      <h3 style={{ margin: '0 0 12px', fontSize: 16 }}>Teams &amp; Collaboration</h3>
      <p style={{ margin: '0 0 12px', lineHeight: 1.6 }}>
        Work together by creating teams and sharing collections.
      </p>
      <ul style={{ margin: 0, paddingLeft: 20, lineHeight: 1.8 }}>
        <li><strong>Create a team</strong> — Click <strong>Teams</strong> in the top bar and enter a team name.</li>
        <li><strong>Invite members</strong> — Enter their email address to send an invitation. Invitations expire after 7 days.</li>
        <li><strong>Roles</strong> — Team members have roles: <strong>Owner</strong>, <strong>Admin</strong>, or <strong>Member</strong>.</li>
        <li><strong>Shared collections</strong> — Mark collections as shared so all team members can access them.</li>
        <li><strong>Invitations</strong> — Check the Invitations tab in the Teams dialog to accept or decline pending invites.</li>
      </ul>
    </div>
  );
}

function AuthHelp() {
  return (
    <div>
      <h3 style={{ margin: '0 0 12px', fontSize: 16 }}>Authentication</h3>
      <p style={{ margin: '0 0 12px', lineHeight: 1.6 }}>
        Restward supports two authentication methods:
      </p>
      <h4 style={{ margin: '16px 0 8px', fontSize: 14 }}>Email &amp; Password (JWT)</h4>
      <ul style={{ margin: 0, paddingLeft: 20, lineHeight: 1.8 }}>
        <li>Register with an email and password.</li>
        <li>Log in to receive a JWT token that authenticates your session.</li>
        <li>Tokens are stored locally and sent as <code style={{ background: 'var(--color-bg-elevated)', padding: '2px 6px', borderRadius: 3, fontSize: 13 }}>Authorization: Bearer &lt;token&gt;</code>.</li>
      </ul>
      <h4 style={{ margin: '16px 0 8px', fontSize: 14 }}>API Key</h4>
      <ul style={{ margin: 0, paddingLeft: 20, lineHeight: 1.8 }}>
        <li>Alternatively, authenticate with an API key.</li>
        <li>API keys are sent via the <code style={{ background: 'var(--color-bg-elevated)', padding: '2px 6px', borderRadius: 3, fontSize: 13 }}>X-Api-Key</code> header.</li>
        <li>An admin API key is generated on first server startup.</li>
      </ul>
    </div>
  );
}

function ShortcutsHelp() {
  return (
    <div>
      <h3 style={{ margin: '0 0 12px', fontSize: 16 }}>Keyboard Shortcuts</h3>
      <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
        {[
          { keys: 'Enter', action: 'Send request (when focused in URL bar)' },
          { keys: 'Esc', action: 'Close dialogs and modals' },
        ].map(({ keys, action }) => (
          <div key={keys} style={{ display: 'flex', alignItems: 'center', gap: 12, fontSize: 13 }}>
            <kbd style={{
              background: 'var(--color-bg-elevated)', border: '1px solid var(--color-border-secondary)',
              borderRadius: 4, padding: '4px 8px', fontFamily: 'inherit', fontSize: 12, minWidth: 60, textAlign: 'center',
            }}>{keys}</kbd>
            <span>{action}</span>
          </div>
        ))}
      </div>
    </div>
  );
}

const tabContent: Record<HelpTab, () => React.ReactElement> = {
  'getting-started': GettingStarted,
  'collections': CollectionsHelp,
  'environments': EnvironmentsHelp,
  'teams': TeamsHelp,
  'auth': AuthHelp,
  'shortcuts': ShortcutsHelp,
};

export function HelpDialog({ onClose }: Props) {
  const [activeTab, setActiveTab] = useState<HelpTab>('getting-started');
  const Content = tabContent[activeTab];

  return (
    <div
      style={{
        position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
        background: 'var(--color-overlay)', display: 'flex', justifyContent: 'center', alignItems: 'center',
        zIndex: 100,
      }}
      onClick={onClose}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        style={{
          background: 'var(--color-bg-secondary)', borderRadius: 8, width: 600, maxHeight: '80vh',
          display: 'flex', overflow: 'hidden', border: '1px solid var(--color-border-secondary)',
        }}
      >
        {/* Sidebar tabs */}
        <div style={{
          width: 160, borderRight: '1px solid var(--color-border-primary)',
          display: 'flex', flexDirection: 'column', flexShrink: 0,
        }}>
          <div style={{
            padding: '12px', borderBottom: '1px solid var(--color-border-primary)',
            fontSize: 14, fontWeight: 600, display: 'flex', justifyContent: 'space-between', alignItems: 'center',
          }}>
            Help
            <button
              onClick={onClose}
              style={{ background: 'none', border: 'none', color: 'var(--color-text-muted)', cursor: 'pointer', fontSize: 18 }}
            >&times;</button>
          </div>
          <div style={{ flex: 1, overflow: 'auto' }}>
            {tabs.map((tab) => (
              <div
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                style={{
                  padding: '10px 12px', cursor: 'pointer', fontSize: 13,
                  background: tab.id === activeTab ? 'var(--color-bg-active)' : 'transparent',
                  color: tab.id === activeTab ? 'var(--color-text-bright)' : 'var(--color-text-secondary)',
                  borderLeft: tab.id === activeTab ? '2px solid var(--color-accent)' : '2px solid transparent',
                }}
              >
                {tab.label}
              </div>
            ))}
          </div>
        </div>

        {/* Content area */}
        <div style={{ flex: 1, overflow: 'auto', padding: 20, color: 'var(--color-text-primary)', fontSize: 13 }}>
          <Content />
        </div>
      </div>
    </div>
  );
}
