import { useEffect, useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Play, Trophy, Users, MessageCircle, ChevronDown, ChevronUp, Trash2, Edit, Film, Vote, ChevronsUpDown } from 'lucide-react';
import anime from 'animejs';
import type { DebatePhase } from '../../types';
import { debatesApi } from '../../api/client';
import { CreateDebateWizard } from './CreateDebateWizard';
import { useApi, useApiMutation } from '../../hooks/useApi';
import { PageHeader } from '../../components/layout';
import { Button, Card, Loading, Modal, DebateLoadingOverlay } from '../../components/ui';
import styles from './DebateDetailPage.module.css';

export function DebateDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const contentRef = useRef<HTMLDivElement>(null);
  const [expandedSections, setExpandedSections] = useState<Set<string>>(new Set(['winner']));
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);

  const { data: debate, loading, error, execute: fetchDebate } = useApi(debatesApi.get);
  const { execute: updateDebate, loading: updating } = useApiMutation(debatesApi.update, {
    onSuccess: (updatedDebate) => {
      setIsEditOpen(false);
      fetchDebate(id!);
    },
  });
  const { execute: runDebate, loading: running } = useApiMutation(debatesApi.run, {
    onSuccess: (result) => {
      fetchDebate(id!);
    },
  });
  const { execute: deleteDebate, loading: deleting } = useApiMutation(debatesApi.delete, {
    onSuccess: () => {
      navigate('/debates');
    },
  });

  useEffect(() => {
    if (id) {
      fetchDebate(id);
    }
  }, [id]);

  useEffect(() => {
    if (contentRef.current && debate) {
      anime({
        targets: contentRef.current.querySelectorAll(`.${styles.section}`),
        opacity: [0, 1],
        translateY: [20, 0],
        duration: 400,
        delay: anime.stagger(80),
        easing: 'easeOutCubic',
      });
    }
  }, [debate]);

  const toggleSection = (section: string) => {
    setExpandedSections((prev) => {
      const next = new Set(prev);
      if (next.has(section)) {
        next.delete(section);
      } else {
        next.add(section);
      }
      return next;
    });
  };

  if (loading && !debate) {
    return (
      <div className={styles.loadingWrapper}>
        <Loading size="lg" text="Loading debate..." />
      </div>
    );
  }

  if (error || !debate) {
    return (
      <div className={styles.errorWrapper}>
        <p>Failed to load debate: {error || 'Not found'}</p>
        <Button onClick={() => navigate('/debates')}>Back to Debates</Button>
      </div>
    );
  }

  const state = debate.state;
  const winner = state?.winner;

  const getStatusBadgeClass = (status: string): string => {
    switch (status.toLowerCase()) {
      case 'created':
        return styles.statusCreated;
      case 'completed':
        return styles.statusCompleted;
      case 'failed':
        return styles.statusFailed;
      default:
        return '';
    }
  };

  return (
    <div className={styles.page}>
      {running && <DebateLoadingOverlay />}

      <div className={styles.topNav}>
        <div className={styles.actionButtons}>
          <Button
            variant="danger"
            size="md"
            icon={<Trash2 size={18} />}
            onClick={() => setIsDeleteModalOpen(true)}
            disabled={running}
          >
            Delete
          </Button>
          {debate.status.toLowerCase() === 'completed' && (
            <Button
              size="md"
              icon={<Film size={18} />}
              onClick={() => navigate(`/debates/${debate.id}/replay`)}
            >
              Replay
            </Button>
          )}
          {debate.status.toLowerCase() === 'created' && (
            <>
              <Button variant="secondary" size="md" icon={<Edit size={18} />} onClick={() => setIsEditOpen(true)}>
                Edit
              </Button>
              <Button size="md" icon={<Play size={18} />} onClick={() => runDebate(debate.id)}>
                Run Debate
              </Button>
            </>
          )}
        </div>
      </div>

      <PageHeader
        title={debate.question}
        description={debate.context || undefined}
      />

      <div className={styles.meta}>
        <span className={`${styles.statusBadge} ${getStatusBadgeClass(debate.status)}`}>
          {debate.status.charAt(0).toUpperCase() + debate.status.slice(1).toLowerCase()}
        </span>
        <span className={styles.metaItem}>
          <Users size={14} />
          {debate.nMembers} members
        </span>
        <span className={styles.metaItem}>
          <MessageCircle size={14} />
          {debate.nRounds} rounds
        </span>
        <div className={styles.metaSpacer} />
        <Button
          variant="ghost"
          size="sm"
          icon={<ChevronsUpDown size={14} />}
          onClick={() => {
            const allSections = ['winner', 'participants', 'initial', 'final', 'ballots'];
            state?.rounds?.forEach((r) => allSections.push(`round-${r.roundNumber}`));
            if (expandedSections.size > 1) {
              setExpandedSections(new Set(['winner']));
            } else {
              setExpandedSections(new Set(allSections));
            }
          }}
        >
          {expandedSections.size > 1 ? 'Collapse all' : 'Expand all'}
        </Button>
      </div>

      <div className={styles.mainLayout}>
        <div ref={contentRef} className={styles.content}>
          {winner && (
            <Card id="winner" className={`${styles.section} ${styles.winnerSection}`} padding="lg">
              <div className={styles.sectionHeader} onClick={() => toggleSection('winner')}>
                <div className={styles.sectionTitle}>
                  <Trophy size={20} className={styles.winnerIcon} />
                  <h2>Winner</h2>
                </div>
                {expandedSections.has('winner') ? <ChevronUp size={20} /> : <ChevronDown size={20} />}
              </div>
              {expandedSections.has('winner') && (
                <div className={styles.winnerContent}>
                  <div className={styles.winnerHeader}>
                    <div>
                      <h3 className={styles.winnerName}>{winner.member.name}</h3>
                      <p className={styles.winnerRole}>{winner.member.role}</p>
                    </div>
                    <div className={styles.winnerScore}>
                      <span className={styles.scoreValue}>{winner.totalPoints}</span>
                      <span className={styles.scoreLabel}>points</span>
                    </div>
                  </div>
                  <div className={styles.winnerStatement}>
                    <h4>Final Statement</h4>
                    <p>{winner.finalStatement}</p>
                  </div>
                </div>
              )}
            </Card>
          )}

          {state?.members && state.members.length > 0 && (
            <Card id="participants" className={styles.section}>
              <div className={styles.sectionHeader} onClick={() => toggleSection('participants')}>
                <div className={styles.sectionTitle}>
                  <Users size={18} />
                  <h2>Participants</h2>
                </div>
                {expandedSections.has('participants') ? <ChevronUp size={20} /> : <ChevronDown size={20} />}
              </div>
              {expandedSections.has('participants') && (
                <div className={styles.participantGrid}>
                  {state.members.map((member, index) => {
                    const result = state.results?.find((r) => r.slot === member.slot);
                    return (
                      <div key={member.slot} className={styles.participant}>
                        <div className={styles.participantRank}>#{result?.rank || index + 1}</div>
                        <div className={styles.participantInfo}>
                          <span className={styles.participantName}>{member.name}</span>
                          <span className={styles.participantRole}>{member.role}</span>
                        </div>
                        {result && (
                          <span className={styles.participantScore}>{result.score} pts</span>
                        )}
                      </div>
                    );
                  })}
                </div>
              )}
            </Card>
          )}

          {state?.initialAnswers && state.initialAnswers.length > 0 && (
            <Card id="initial" className={styles.section}>
              <div className={styles.sectionHeader} onClick={() => toggleSection('initial')}>
                <div className={styles.sectionTitle}>
                  <MessageCircle size={18} />
                  <h2>Initial Answers</h2>
                </div>
                {expandedSections.has('initial') ? <ChevronUp size={20} /> : <ChevronDown size={20} />}
              </div>
              {expandedSections.has('initial') && (
                <div className={styles.answers}>
                  {state.initialAnswers.map((answer) => {
                    const member = state.members.find((m) => m.slot === answer.slot);
                    return (
                      <div key={answer.slot} className={styles.answer}>
                        <div className={styles.answerHeader}>
                          <span className={styles.answerAuthor}>{member?.name}</span>
                          <span className={styles.answerRole}>{member?.role}</span>
                        </div>
                        <p className={styles.answerText}>{answer.text}</p>
                      </div>
                    );
                  })}
                </div>
              )}
            </Card>
          )}

          {state?.rounds && state.rounds.length > 0 && (
            state.rounds.map((round) => (
              <Card key={round.roundNumber} id={`round-${round.roundNumber}`} className={styles.section}>
                <div className={styles.sectionHeader} onClick={() => toggleSection(`round-${round.roundNumber}`)}>
                  <div className={styles.sectionTitle}>
                    <MessageCircle size={18} />
                    <h2>Round {round.roundNumber}</h2>
                  </div>
                  {expandedSections.has(`round-${round.roundNumber}`) ? <ChevronUp size={20} /> : <ChevronDown size={20} />}
                </div>
                {expandedSections.has(`round-${round.roundNumber}`) && (
                  <div className={styles.roundContent}>
                    {round.questions.map((q, i) => {
                      const from = state.members.find((m) => m.slot === q.fromSlot);
                      const to = state.members.find((m) => m.slot === q.toSlot);
                      const answer = round.answers.find((a) => a.slot === q.toSlot);
                      const answerMember = answer ? state.members.find((m) => m.slot === answer.slot) : null;
                      return (
                        <div key={i} className={styles.qaBlock}>
                          <div className={styles.qaItem}>
                            <span className={styles.qaLabel}>Question</span>
                            <div className={styles.exchange}>
                              <div className={styles.exchangeHeader}>
                                <span className={styles.exchangeFrom}>{from?.name}</span>
                                <span className={styles.exchangeArrow}>asks</span>
                                <span className={styles.exchangeTo}>{to?.name}</span>
                              </div>
                              <p className={styles.exchangeText}>{q.text}</p>
                            </div>
                          </div>
                          {answer && (
                            <div className={styles.qaItem}>
                              <span className={styles.qaLabel}>Answer</span>
                              <div className={styles.answer}>
                                <div className={styles.answerHeader}>
                                  <span className={styles.answerAuthor}>{answerMember?.name}</span>
                                </div>
                                <p className={styles.answerText}>{answer.text}</p>
                              </div>
                            </div>
                          )}
                        </div>
                      );
                    })}

                    {round.amendments.length > 0 && (
                      <div className={styles.roundSection}>
                        <h4>Amendments</h4>
                        <div className={styles.amendmentsList}>
                          {round.amendments.map((a, i) => {
                            const member = state.members.find((m) => m.slot === a.slot);
                            return (
                              <div key={i} className={styles.answer}>
                                <div className={styles.answerHeader}>
                                  <span className={styles.answerAuthor}>{member?.name}</span>
                                </div>
                                <p className={styles.answerText}>{a.text}</p>
                              </div>
                            );
                          })}
                        </div>
                      </div>
                    )}
                  </div>
                )}
              </Card>
            ))
          )}

          {state?.finalStatements && state.finalStatements.length > 0 && (
            <Card id="final" className={styles.section}>
              <div className={styles.sectionHeader} onClick={() => toggleSection('final')}>
                <div className={styles.sectionTitle}>
                  <MessageCircle size={18} />
                  <h2>Final Statements</h2>
                </div>
                {expandedSections.has('final') ? <ChevronUp size={20} /> : <ChevronDown size={20} />}
              </div>
              {expandedSections.has('final') && (
                <div className={styles.answers}>
                  {state.finalStatements.map((statement) => {
                    const member = state.members.find((m) => m.slot === statement.slot);
                    return (
                      <div key={statement.slot} className={styles.answer}>
                        <div className={styles.answerHeader}>
                          <span className={styles.answerAuthor}>{member?.name}</span>
                          <span className={styles.answerRole}>{member?.role}</span>
                        </div>
                        <p className={styles.answerText}>{statement.text}</p>
                      </div>
                    );
                  })}
                </div>
              )}
            </Card>
          )}

          {state?.ballots && state.ballots.length > 0 && (
            <Card id="ballots" className={styles.section}>
              <div className={styles.sectionHeader} onClick={() => toggleSection('ballots')}>
                <div className={styles.sectionTitle}>
                  <Vote size={18} />
                  <h2>Ballots</h2>
                </div>
                {expandedSections.has('ballots') ? <ChevronUp size={20} /> : <ChevronDown size={20} />}
              </div>
              {expandedSections.has('ballots') && (
                <div className={styles.ballotsGrid}>
                  {state.ballots.map((ballot) => {
                    const voter = state.members.find((m) => m.slot === ballot.slot);
                    const sortedRankings = [...(ballot.ranks || [])].sort((a, b) => a.rank - b.rank);
                    return (
                      <div key={ballot.slot} className={styles.ballotCard}>
                        <div className={styles.ballotHeader}>
                          <span className={styles.ballotVoter}>{voter?.name}</span>
                          <span className={styles.ballotVoterRole}>{voter?.role}</span>
                        </div>
                        <div className={styles.ballotRankings}>
                          {sortedRankings.map((ranking) => {
                            const rankedMember = state.members.find((m) => m.slot === ranking.slot);
                            const isSelf = ranking.slot === ballot.slot;
                            return (
                              <div
                                key={ranking.slot}
                                className={`${styles.ballotRank} ${isSelf ? styles.ballotRankSelf : ''}`}
                              >
                                <span className={styles.rankNumber}>#{ranking.rank}</span>
                                <span className={styles.rankName}>{rankedMember?.name}</span>
                                {isSelf && <span className={styles.selfBadge}>(self)</span>}
                              </div>
                            );
                          })}
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </Card>
          )}
        </div>

        <nav className={styles.stickyNav}>
          {winner && (
            <a href="#winner" className={styles.navLink}>Winner</a>
          )}
          {state?.members && state.members.length > 0 && (
            <a href="#participants" className={styles.navLink}>Participants</a>
          )}
          {state?.initialAnswers && state.initialAnswers.length > 0 && (
            <a href="#initial" className={styles.navLink}>Initial Answers</a>
          )}
          {state?.rounds?.map((round) => (
            <a key={round.roundNumber} href={`#round-${round.roundNumber}`} className={styles.navLink}>
              Round {round.roundNumber}
            </a>
          ))}
          {state?.finalStatements && state.finalStatements.length > 0 && (
            <a href="#final" className={styles.navLink}>Final Statements</a>
          )}
          {state?.ballots && state.ballots.length > 0 && (
            <a href="#ballots" className={styles.navLink}>Ballots</a>
          )}
        </nav>
      </div>
      <Modal
        isOpen={isDeleteModalOpen}
        onClose={() => setIsDeleteModalOpen(false)}
        title="Delete Debate"
      >
        <div className={styles.deleteConfirm}>
          <p>
            Are you sure you want to delete this debate? This action cannot be undone.
          </p>
          <div className={styles.deleteActions} style={{ display: 'flex', justifyContent: 'flex-end', gap: '1rem', marginTop: '1.5rem' }}>
            <Button variant="secondary" onClick={() => setIsDeleteModalOpen(false)}>
              Cancel
            </Button>
            <Button variant="danger" onClick={() => debate && deleteDebate(debate.id)} loading={deleting}>
              Delete
            </Button>
          </div>
        </div>
      </Modal>

      <Modal
        isOpen={isEditOpen}
        onClose={() => setIsEditOpen(false)}
        title="Edit Debate"
        size="lg"
      >
        {debate && (
          <CreateDebateWizard
            initialData={debate}
            onSubmit={(data) => updateDebate(debate.id, data)}
            onCancel={() => setIsEditOpen(false)}
            loading={updating}
          />
        )}
      </Modal>
    </div>
  );
}
