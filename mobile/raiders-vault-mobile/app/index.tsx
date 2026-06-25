import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { LiveOpsCard } from '../components/LiveOpsCard';
import { fallbackLiveOps } from '../lib/liveOps';

export default function HomeScreen() {
  return (
    <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
      <Text style={styles.eyebrow}>Raiders Vault Mobile</Text>
      <Text style={styles.title}>Pocket Ops Console</Text>
      <Text style={styles.copy}>
        Mobile-first companion shell for live ARC Raiders conditions, item readiness, and operator planning.
      </Text>

      {fallbackLiveOps.map(item => (
        <LiveOpsCard item={item} key={`${item.map}-${item.condition}`} />
      ))}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: '#071019'
  },
  content: {
    padding: 22,
    gap: 14
  },
  eyebrow: {
    color: '#67e8f9',
    fontSize: 12,
    fontWeight: '900',
    letterSpacing: 1.5,
    textTransform: 'uppercase'
  },
  title: {
    color: '#edf7ff',
    fontSize: 42,
    fontWeight: '900',
    lineHeight: 44
  },
  copy: {
    color: '#a7b7c9',
    fontSize: 16,
    lineHeight: 24
  }
});
