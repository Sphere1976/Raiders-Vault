export type MobileCondition = {
  map: string;
  condition: string;
  status: string;
  recommendation: string;
};

export const fallbackLiveOps: MobileCondition[] = [
  {
    map: 'The Blue Gate',
    condition: 'Harvester',
    status: 'Active',
    recommendation: 'Check extraction distance before committing to high-value mechanical routes.'
  },
  {
    map: 'Dam Battlegrounds',
    condition: 'Night Raid',
    status: 'Watch',
    recommendation: 'Favor quiet loadouts, smoke utility, and short objective windows.'
  }
];
