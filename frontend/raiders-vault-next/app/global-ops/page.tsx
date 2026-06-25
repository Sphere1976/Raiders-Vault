import { GlobalOpsDashboard } from '../../components/GlobalOpsDashboard';
import { getGlobalOps } from '../../lib/raidersVaultApi';

export default async function GlobalOpsPage() {
  const data = await getGlobalOps();

  return <GlobalOpsDashboard data={data} />;
}
