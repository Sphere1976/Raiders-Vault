/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  reactStrictMode: true,
  env: {
    RAIDERS_VAULT_API_URL: process.env.RAIDERS_VAULT_API_URL ?? 'http://127.0.0.1:5217'
  }
};

export default nextConfig;
