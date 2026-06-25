import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  thresholds: {
    http_req_failed: ['rate<0.02'],
    http_req_duration: ['p(95)<750']
  },
  scenarios: {
    smoke: {
      executor: 'constant-vus',
      vus: 10,
      duration: '1m'
    }
  }
};

const baseUrl = __ENV.BASE_URL || 'http://127.0.0.1:5217';

export default function () {
  const health = http.get(`${baseUrl}/health`);
  check(health, {
    'health status is 200': response => response.status === 200
  });

  const database = http.get(`${baseUrl}/Database/Index`);
  check(database, {
    'database responds without server error': response => response.status < 500
  });

  sleep(1);
}
