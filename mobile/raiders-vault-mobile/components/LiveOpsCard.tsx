import { StyleSheet, Text, View } from 'react-native';
import type { MobileCondition } from '../lib/liveOps';

type Props = {
  item: MobileCondition;
};

export function LiveOpsCard({ item }: Props) {
  return (
    <View style={styles.card}>
      <Text style={styles.status}>{item.status}</Text>
      <Text style={styles.title}>{item.condition}</Text>
      <Text style={styles.map}>{item.map}</Text>
      <Text style={styles.copy}>{item.recommendation}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,.14)',
    backgroundColor: 'rgba(255,255,255,.06)',
    borderRadius: 10,
    padding: 16
  },
  status: {
    color: '#86efac',
    fontWeight: '900',
    textTransform: 'uppercase'
  },
  title: {
    color: '#edf7ff',
    fontSize: 24,
    fontWeight: '900',
    marginTop: 8
  },
  map: {
    color: '#67e8f9',
    fontWeight: '800',
    marginTop: 4
  },
  copy: {
    color: '#a7b7c9',
    fontSize: 15,
    lineHeight: 22,
    marginTop: 10
  }
});
