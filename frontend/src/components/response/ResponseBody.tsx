import Editor from '@monaco-editor/react';
import { useTheme } from '../../hooks/useTheme';

interface Props {
  body: string;
  headers: Record<string, string>;
}

function detectLanguage(body: string, headers: Record<string, string>): string {
  const contentType = headers['Content-Type'] || headers['content-type'] || '';
  if (contentType.includes('json') || body.trimStart().startsWith('{') || body.trimStart().startsWith('[')) return 'json';
  if (contentType.includes('xml') || body.trimStart().startsWith('<')) return 'xml';
  if (contentType.includes('html')) return 'html';
  return 'plaintext';
}

function tryFormatJson(body: string): string {
  try {
    return JSON.stringify(JSON.parse(body), null, 2);
  } catch {
    return body;
  }
}

export function ResponseBody({ body, headers }: Props) {
  const language = detectLanguage(body, headers);
  const formatted = language === 'json' ? tryFormatJson(body) : body;
  const { theme } = useTheme();

  return (
    <div style={{ height: '100%' }}>
      <Editor
        height="100%"
        language={language}
        value={formatted}
        theme={theme === 'dark' ? 'vs-dark' : 'light'}
        options={{
          readOnly: true,
          minimap: { enabled: false },
          fontSize: 13,
          lineNumbers: 'on',
          scrollBeyondLastLine: false,
          wordWrap: 'on',
          tabSize: 2,
        }}
      />
    </div>
  );
}
