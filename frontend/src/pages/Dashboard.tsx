import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Users, UserMinus, UserCheck, Play, ArrowRight, Trophy, BookOpen, Plus } from 'lucide-react';
import anime from 'animejs';
import { membersApi, debatesApi, dashboardApi } from '../api/client';
import { useApi, useApiMutation } from '../hooks/useApi';
import { PageHeader } from '../components/layout';
import { Card, Button, Badge, Loading, Modal, LoadingOverlay } from '../components/ui';
import type { CreateDebateRequest, CreateMemberRequest } from '../types';
import { CreateDebateWizard } from './debates/CreateDebateWizard';
import { MemberForm } from './members/MemberForm';
import styles from './Dashboard.module.css';

export function Dashboard() {
  const navigate = useNavigate();
  const leftColRef = useRef<HTMLDivElement>(null);
  const rightColRef = useRef<HTMLDivElement>(null);

  const [isCreateDebateOpen, setIsCreateDebateOpen] = useState(false);
  const [isCreateMemberOpen, setIsCreateMemberOpen] = useState(false);
  const [runningDebateId, setRunningDebateId] = useState<string | null>(null);

  // Consolidated Stats Data
  const { data: stats, execute: fetchStats } = useApi(dashboardApi.getStats);

  // Lists Data
  const { data: topMembers, execute: fetchTopMembers } = useApi(membersApi.list);
  const { data: recentDebates, execute: fetchRecentDebates } = useApi(debatesApi.list);

  const { execute: createDebate, loading: creatingDebate } = useApiMutation(debatesApi.create, {
    onSuccess: () => {
      setIsCreateDebateOpen(false);
      fetchStats();
      fetchRecentDebates({ pageSize: 5 });
    },
  });

  const { execute: createMember, loading: creatingMember } = useApiMutation(membersApi.create, {
    onSuccess: () => {
      setIsCreateMemberOpen(false);
      fetchStats();
      fetchTopMembers({ pageSize: 5, sortBy: 'PointsDesc', status: 'Active' });
    },
  });

  const { execute: runDebate } = useApiMutation(debatesApi.run, {
    onSuccess: (debate) => {
      setRunningDebateId(null);
      if (debate && debate.id) {
        navigate(`/debates/${debate.id}`);
      }
    },
    onError: () => {
      setRunningDebateId(null);
    },
  });

  useEffect(() => {
    fetchStats();
    fetchTopMembers({ pageSize: 5, sortBy: 'PointsDesc', status: 'Active' });
    fetchRecentDebates({ pageSize: 5 });
  }, []);

  useEffect(() => {
    // Animation for Left Column
    if (leftColRef.current) {
      anime({
        targets: leftColRef.current.children,
        opacity: [0, 1],
        translateY: [20, 0],
        duration: 500,
        delay: anime.stagger(100),
        easing: 'easeOutCubic',
      });
    }

    // Animation for Right Column
    if (rightColRef.current) {
      anime({
        targets: rightColRef.current.children,
        opacity: [0, 1],
        translateY: [20, 0],
        duration: 500,
        delay: anime.stagger(100, { start: 200 }),
        easing: 'easeOutCubic',
      });
    }
  }, [stats]);

  const topMembersList = topMembers?.items ?? [];
  const recentDebatesList = recentDebates ?? [];

  const handleRunDebate = (e: React.MouseEvent, debateId: string) => {
    e.stopPropagation();
    setRunningDebateId(debateId);
    runDebate(debateId);
  };

  const memberStats = [
    {
      label: 'Total Members',
      value: stats?.members.total ?? 0,
      icon: Users,
      onClick: () => navigate('/members'),
    },
    {
      label: 'Active Members',
      value: stats?.members.active ?? 0,
      icon: UserCheck,
      onClick: () => navigate('/members?status=Active'),
    },
    {
      label: 'Benched Members',
      value: stats?.members.benched ?? 0,
      icon: UserMinus,
      onClick: () => navigate('/members?status=Benched'),
    },
    {
      label: 'Retired',
      value: stats?.members.retired ?? 0,
      icon: Users,
      onClick: () => navigate('/members?status=Retired'),
    },
  ];

  const debateStats = [
    {
      label: 'Total Debates',
      value: stats?.debates.total ?? 0,
      icon: BookOpen,
      onClick: () => navigate('/debates'),
    },
    {
      label: 'Ready to Start',
      value: stats?.debates.ready ?? 0,
      icon: Play,
      onClick: () => navigate('/debates?status=Created'),
    },
    {
      label: 'Completed',
      value: stats?.debates.completed ?? 0,
      icon: Trophy,
      onClick: () => navigate('/debates?status=Completed'),
    },
    {
      label: 'Failed',
      value: stats?.debates.failed ?? 0,
      icon: UserMinus,
      onClick: () => navigate('/debates?status=Failed'),
    },
  ];

  return (
    <div className={styles.page}>
      {runningDebateId && <LoadingOverlay text="Running debate..." />}

      <PageHeader
        title="Dashboard"
        action={
          <div style={{ display: 'flex', gap: '0.75rem' }}>
            <Button icon={<Users size={18} />} variant="secondary" onClick={() => setIsCreateMemberOpen(true)}>
              Add Member
            </Button>
            <Button icon={<Plus size={18} />} onClick={() => setIsCreateDebateOpen(true)}>
              Add Debate
            </Button>
          </div>
        }
      />

      <div className={styles.mainGrid}>
        {/* LEFT COLUMN: MEMBERS */}
        <div className={styles.column} ref={leftColRef}>
          <div className={styles.statsGrid}>
            {memberStats.map((stat, i) => (
              <Card
                key={i}
                className={styles.statCard}
                onClick={stat.onClick}
              >
                <div className={styles.statHeader}>
                  <div className={styles.statIcon}>
                    <stat.icon size={20} />
                  </div>
                  <span className={styles.statValue}>{stat.value}</span>
                </div>
                <span className={styles.statLabel}>{stat.label}</span>
              </Card>
            ))}
          </div>

          <div className={styles.sectionHeader}>
            <h3>Top Performers</h3>
          </div>

          <Card padding="none">
            {!topMembers ? (
              <div className={styles.loadingSection}>
                <Loading size="sm" />
              </div>
            ) : topMembersList.length === 0 ? (
              <div className={styles.emptySection}>
                <p>No active members yet</p>
              </div>
            ) : (
              <div className={styles.leaderboard}>
                {topMembersList.map((member, index) => (
                  <div
                    key={member.id}
                    className={styles.leaderboardItem}
                    onClick={() => navigate(`/members/${member.id}`)}
                    style={{ cursor: 'pointer' }}
                  >
                    <span className={styles.rank}>#{index + 1}</span>
                    <div className={styles.memberInfo}>
                      <span className={styles.memberName}>{member.name}</span>
                      <span className={styles.memberRole}>{member.role}</span>
                    </div>
                    <div className={styles.memberStats}>
                      <span className={styles.memberWins}>{member.winsCount} wins</span>
                      <span className={styles.memberPoints}>{member.points} pts</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>


        </div>

        {/* RIGHT COLUMN: DEBATES */}
        <div className={styles.column} ref={rightColRef}>
          {/* Debate Stats & Actions Grid */}
          <div className={styles.statsGrid}>
            {debateStats.map((stat, i) => (
              <Card
                key={i}
                className={styles.statCard}
                onClick={stat.onClick}
              >
                <div className={styles.statHeader}>
                  <div className={styles.statIcon}>
                    <stat.icon size={20} />
                  </div>
                  <span className={styles.statValue}>{stat.value}</span>
                </div>
                <span className={styles.statLabel}>{stat.label}</span>
              </Card>
            ))}
          </div>

          <div className={styles.sectionHeader}>
            <h3>Recent Debates</h3>
          </div>

          <Card padding="none">
            {!recentDebates ? (
              <div className={styles.loadingSection}>
                <Loading size="sm" />
              </div>
            ) : recentDebatesList.length === 0 ? (
              <div className={styles.emptySection}>
                <p>No debates found</p>
                <Button size="sm" onClick={() => setIsCreateDebateOpen(true)}>
                  Create a debate
                </Button>
              </div>
            ) : (
              <div className={styles.debateList}>
                {recentDebatesList.map((debate) => (
                  <div
                    key={debate.id}
                    className={styles.debateItem}
                    onClick={() => navigate(`/debates/${debate.id}`)}
                  >
                    <div className={styles.debateContent}>
                      <span className={styles.debateQuestion}>{debate.question}</span>
                      <div className={styles.debateMeta}>
                        <span>{debate.nMembers} members</span>
                        <span>{debate.state?.rounds?.length ?? 0}/{debate.nRounds} rounds</span>
                        <MemberBadge debate={debate} />
                      </div>
                    </div>
                    {debate.status.toLowerCase() === 'created' ? (
                      <Button
                        size="sm"
                        variant="ghost"
                        icon={<Play size={14} />}
                        onClick={(e) => handleRunDebate(e, debate.id)}
                        className={styles.actionButton}
                        aria-label="Start debate"
                      />
                    ) : (
                      <ArrowRight size={16} className={styles.debateArrow} />
                    )}
                  </div>
                ))}
              </div>
            )}
          </Card>
        </div>
      </div>

      <Modal
        isOpen={isCreateDebateOpen}
        onClose={() => setIsCreateDebateOpen(false)}
        title="Create New Debate"
        size="lg"
      >
        <CreateDebateWizard
          onSubmit={createDebate}
          onCancel={() => setIsCreateDebateOpen(false)}
          loading={creatingDebate}
        />
      </Modal>

      <Modal
        isOpen={isCreateMemberOpen}
        onClose={() => setIsCreateMemberOpen(false)}
        title="Create New Member"
        size="lg"
      >
        <MemberForm
          onSubmit={createMember}
          onCancel={() => setIsCreateMemberOpen(false)}
          loading={creatingMember}
        />
      </Modal>
    </div>
  );
}

// Helper component for debate status/winner
function MemberBadge({ debate }: { debate: any }) {
  if (debate.state?.winner) {
    return (
      <span className={styles.winnerBadge}>
        <Trophy size={12} />
        {debate.state.winner.member.name}
      </span>
    );
  }
  if (debate.status === 'Created') {
    return <span className={styles.readyBadge}>Ready to start</span>;
  }
  return null;
}
