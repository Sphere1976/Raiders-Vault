import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 20 },
    { duration: '1m', target: 20 },
    { duration: '30s', target: 0 }
  ],
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1200']
  }
};

const baseUrl = __ENV.BASE_URL || 'http://127.0.0.1:5217';

export default function () {
  const response = http.get(`${baseUrl}/GlobalOps/Index`, {
    redirects: 0
  });

  check(response, {
    'global ops is protected or available': r => [200, 302, 401].includes(r.status),
    'no server error': r => r.status < 500
  });

  sleep(1);
}
