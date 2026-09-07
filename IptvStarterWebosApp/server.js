const express = require('express');
const path = require('path');

const app = express();
const port = 3000;

app.use(express.static(path.join(__dirname, 'src')));

app.get('/api/channels', (req, res) => {
  res.json({
    channels: [
      { id: '1', name: 'Canal 1', url: 'https://example.com/stream1.ts' },
      { id: '2', name: 'Canal 2', url: 'https://example.com/stream2.ts' },
      { id: '3', name: 'Canal 3', url: 'https://example.com/stream3.ts' }
    ]
  });
});

app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'src', 'index.html'));
});

app.listen(port, () => {
  console.log(`Servidor rodando em http://localhost:${port}`);
});
