// Adapted from jfin602/space-factory at ce163753d8973ccd018f2344c6175c52fe4b113d: scripts/validate-space-factory-phase.mjs
// Ported from jfin602/dope at 02bb13779acb7e2409f8c6ee42aeffccc0180c37: scripts/validate-codex-phase.mjs
import { readdir, readFile } from 'node:fs/promises';
import path from 'node:path';

import { buildPlan, roadmapFamilyLabel } from './fish-phase-core.mjs';

const folderName = process.argv[2];
if (!folderName || process.argv.length !== 3) {
  throw new Error('Usage: npm run fish:phase:validate -- <task-folder>');
}

const taskDirectory = path.join(process.cwd(), 'docs', 'tasks', folderName);
const names = await readdir(taskDirectory);
const textFiles = names.filter((name) => name.toLowerCase().endsWith('.txt'));
const entries = await Promise.all(
  textFiles.map(async (filename) => ({
    filename,
    text: await readFile(path.join(taskDirectory, filename), 'utf8'),
  })),
);
const plan = buildPlan(entries, folderName);

console.log(
  plan.mode === 'correction'
    ? `Correction stack ${plan.folderName} (roadmap phase ${plan.phase}) prompt grammar: VALID`
    : `${roadmapFamilyLabel(plan.roadmapFamily)} Phase ${plan.phase} prompt grammar: VALID`,
);
if (plan.mode === 'correction') {
  console.log(`Required unchanged version: ${plan.unchangedVersion}`);
}
for (const prompt of plan.prompts) {
  console.log(
    `P${prompt.number} | ${prompt.kind} | ${prompt.recommendation} | Browser required: ${prompt.browserRequired ? 'yes' : 'no'} | ${prompt.mode === 'correction' ? `${prompt.unchangedVersion} (UNCHANGED)` : prompt.targetVersion} | ${prompt.filename}`,
  );
}
console.log(`Manual closeout: P${plan.closeout.number}`);
