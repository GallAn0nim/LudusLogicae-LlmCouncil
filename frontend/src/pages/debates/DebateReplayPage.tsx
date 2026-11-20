import { useState, useEffect, useRef, useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { debatesApi } from '../../api/client';
import type { Debate, RoundQuestion, RoundAnswer, RoundAmendment } from '../../types';
import { Button, Loading } from '../../components/ui';
import { Play, Pause, Gauge, Calendar, MessageSquare, FileText, Users, Trophy, Activity } from 'lucide-react';
import styles from './DebateReplayPage.module.css';

interface RevealedMember {
    slot: number;
    name: string;
    role: string;
    hasIntroduced: boolean;
    points: number;
}

interface Message {
    id: string;
    type: 'system' | 'member' | 'vote';
    slot?: number;
    name: string;
    role?: string;
    content: string;
    pointsChange?: { slot: number; delta: number }[];
}

export function DebateReplayPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [debate, setDebate] = useState<Debate | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [messages, setMessages] = useState<Message[]>([]);
    const [visibleMessages, setVisibleMessages] = useState<Message[]>([]);
    const [currentIndex, setCurrentIndex] = useState(0);
    const [isPlaying, setIsPlaying] = useState(true);
    const [isTyping, setIsTyping] = useState(false);
    const [speed, setSpeed] = useState(2500);
    const [showSpeedMenu, setShowSpeedMenu] = useState(false);
    const [isContextExpanded, setIsContextExpanded] = useState(false);

    const messagesEndRef = useRef<HTMLDivElement>(null);
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!id) {
          return;
        }

        debatesApi
            .get(id)
            .then(setDebate)
            .catch((err: Error) => setError(err.message))
            .finally(() => setLoading(false));
    }, [id]);

    const formatDate = (dateString: string) => {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    };

    useEffect(() => {
        if (!debate || !debate.state) {
          return;
        }

        const allMessages: Message[] = [];
        const state = debate.state;

        allMessages.push({
            id: 'welcome',
            type: 'system',
            name: 'Ludus Logicae',
            content: `Welcome! Today is ${formatDate(debate.createdAt)}.`
        });

        allMessages.push({
            id: 'topic',
            type: 'system',
            name: 'Ludus Logicae',
            content: `Today's debate topic: "${debate.question}"`
        });

        if (debate.context) {
            allMessages.push({
                id: 'context',
                type: 'system',
                name: 'Ludus Logicae',
                content: `Context: ${debate.context}`
            });
        }

        if (state.members && state.members.length > 0) {
            allMessages.push({
                id: 'intro-participants',
                type: 'system',
                name: 'Ludus Logicae',
                content: `Let me introduce our ${state.members.length} distinguished participants. Please introduce yourselves!`
            });

            state.members.forEach((member) => {
                allMessages.push({
                    id: `intro-${member.slot}`,
                    type: 'member',
                    slot: member.slot,
                    name: member.name,
                    role: member.role,
                    content: member.introduction || `Hello! I'm ${member.name}, and I'll be representing the ${member.role} perspective in today's debate.`
                });
            });

            allMessages.push({
                id: 'intro-complete',
                type: 'system',
                name: 'Ludus Logicae',
                content: `Excellent! Now let's begin the debate.`
            });
        }

        if (state.initialAnswers && state.initialAnswers.length > 0) {
            allMessages.push({
                id: 'phase-initial',
                type: 'system',
                name: 'Ludus Logicae',
                content: `📢 Phase 1: Initial Answers - Each participant will share their opening statement.`
            });

            state.initialAnswers.forEach((answer) => {
                const member = state.members.find((m) => m.slot === answer.slot);
                if (member) {
                    allMessages.push({
                        id: `initial-${answer.slot}`,
                        type: 'member',
                        slot: member.slot,
                        name: member.name,
                        role: member.role,
                        content: answer.text
                    });
                }
            });

            allMessages.push({
                id: 'initial-complete',
                type: 'system',
                name: 'Ludus Logicae',
                content: `Great opening statements! Now let's move to the cross-examination rounds.`
            });
        }

        if (state.rounds) {
            state.rounds.forEach((round) => {
                allMessages.push({
                    id: `phase-round-${round.roundNumber}`,
                    type: 'system',
                    name: 'Ludus Logicae',
                    content: `📢 Round ${round.roundNumber}: Cross-Examination - Participants will now question each other.`
                });

                const questions = round.questions || [];
                const answers = round.answers || [];

                questions.forEach((question: RoundQuestion, idx: number) => {
                    const fromMember = state.members.find((m) => m.slot === question.fromSlot);
                    const toMember = state.members.find((m) => m.slot === question.toSlot);

                    if (fromMember && toMember) {
                        allMessages.push({
                            id: `q-${round.roundNumber}-${idx}`,
                            type: 'member',
                            slot: fromMember.slot,
                            name: fromMember.name,
                            role: `asks ${toMember.name}`,
                            content: question.text
                        });

                        const answer = answers.find((a: RoundAnswer) => a.slot === question.toSlot);
                        if (answer) {
                            allMessages.push({
                                id: `r-${round.roundNumber}-${idx}-${answer.slot}`,
                                type: 'member',
                                slot: toMember.slot,
                                name: toMember.name,
                                role: 'responds',
                                content: answer.text
                            });
                        }
                    }
                });

                if (round.amendments && round.amendments.length > 0) {
                    allMessages.push({
                        id: `phase-amend-${round.roundNumber}`,
                        type: 'system',
                        name: 'Ludus Logicae',
                        content: `Participants may now amend their positions based on the discussion.`
                    });

                    round.amendments.forEach((amendment: RoundAmendment, idx: number) => {
                        const member = state.members.find((m) => m.slot === amendment.slot);
                        if (member) {
                            allMessages.push({
                                id: `a-${round.roundNumber}-${idx}`,
                                type: 'member',
                                slot: member.slot,
                                name: member.name,
                                role: 'amends position',
                                content: amendment.text
                            });
                        }
                    });
                }
            });
        }

        const hasVotingData = (state.ballots && state.ballots.length > 0) || (state.results && state.results.length > 0);
        if (hasVotingData) {
            allMessages.push({
                id: 'phase-voting',
                type: 'system',
                name: 'Ludus Logicae',
                content: `🗳️ Voting Phase - Each participant will now rank their peers using Borda count voting.`
            });

            const memberCount = state.members.length;

            const hasRankingsData = state.ballots && state.ballots.some(b => b.ranks && b.ranks.length > 0);

            if (hasRankingsData && state.ballots) {
                state.ballots.forEach((ballot) => {
                    const voter = state.members.find(m => m.slot === ballot.slot);
                    if (!voter || !ballot.ranks || ballot.ranks.length === 0) {
                      return;
                    }

                    const sortedRankings = [...ballot.ranks].sort((a, b) => a.rank - b.rank);
                    const rankingText = sortedRankings
                        .map(r => {
                            const ranked = state.members.find(m => m.slot === r.slot);
                            const points = memberCount - r.rank;
                            const isSelf = r.slot === ballot.slot;
                            return `${r.rank}. ${ranked?.name || 'Unknown'} (+${points} pts)${isSelf ? ' (self)' : ''}`;
                        })
                        .join('\n');

                    const pointsChange = sortedRankings.map(r => ({
                        slot: r.slot,
                        delta: memberCount - r.rank
                    }));

                    allMessages.push({
                        id: `vote-${ballot.slot}`,
                        type: 'vote',
                        slot: ballot.slot,
                        name: voter.name,
                        role: 'casts vote',
                        content: rankingText,
                        pointsChange
                    });
                });
            } else if (state.results && state.results.length > 0) {
                state.members.forEach((voter) => {
                    const voterPoints: { [slot: number]: number } = {};

                    const sortedByFinalScore = [...state.results].sort((a, b) => a.rank - b.rank);
                    sortedByFinalScore.forEach((result) => {
                        const pointsGiven = memberCount - result.rank;
                        voterPoints[result.slot] = pointsGiven;
                    });

                    const rankingText = sortedByFinalScore
                        .map((r) => {
                            const ranked = state.members.find(m => m.slot === r.slot);
                            const pts = memberCount - r.rank;
                            const isSelf = r.slot === voter.slot;
                            return `${r.rank}. ${ranked?.name || 'Unknown'} (+${pts} pts)${isSelf ? ' (self)' : ''}`;
                        })
                        .join('\n');

                    const pointsChange = Object.entries(voterPoints).map(([slot, delta]) => ({
                        slot: parseInt(slot),
                        delta
                    }));

                    allMessages.push({
                        id: `vote-${voter.slot}`,
                        type: 'vote',
                        slot: voter.slot,
                        name: voter.name,
                        role: 'casts vote',
                        content: rankingText,
                        pointsChange
                    });
                });
            }

            allMessages.push({
                id: 'voting-complete',
                type: 'system',
                name: 'Ludus Logicae',
                content: `All votes have been cast! Let's tally the results...`
            });
        }

        if (state.winner) {
            const sortedResults = [...(state.results || [])].sort((a, b) => a.rank - b.rank);
            const standingsText = sortedResults.map(r => {
                const member = state.members.find(m => m.slot === r.slot);
                const medal = r.rank === 1 ? '🥇' : r.rank === 2 ? '🥈' : r.rank === 3 ? '🥉' : `${r.rank}.`;
                return `${medal} ${member?.name || 'Unknown'} - ${r.score} points`;
            }).join('\n');

            allMessages.push({
                id: 'results',
                type: 'system',
                name: 'Ludus Logicae',
                content: `🏆 Final Results:\n\n${standingsText}`
            });

            allMessages.push({
                id: 'winner',
                type: 'system',
                name: 'Ludus Logicae',
                content: `Congratulations to ${state.winner.member.name} for winning this debate! 🎉`
            });
        }

        allMessages.push({
            id: 'closing',
            type: 'system',
            name: 'Ludus Logicae',
            content: `Thank you for watching this debate. We hope you enjoyed it!`
        });

        setMessages(allMessages);
    }, [debate]);

    useEffect(() => {
        if (!isPlaying || currentIndex >= messages.length) {
          return;
        }

        setIsTyping(true);
        const timer = setTimeout(() => {
            setVisibleMessages((prev) => [...prev, messages[currentIndex]]);
            setCurrentIndex((prev) => prev + 1);
            setIsTyping(false);
        }, speed);

        return () => clearTimeout(timer);
    }, [currentIndex, isPlaying, messages, speed]);

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [visibleMessages]);

    const getMemberColor = (slot: number) => {
        const colors = ['#f97316', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899', '#06b6d4', '#84cc16'];
        return colors[slot % colors.length];
    };

    const getSystemColor = () => '#6366f1';

    const getMemberInitials = (name: string) => {
        return name
            .split(' ')
            .map((n) => n[0])
            .join('')
            .toUpperCase()
            .slice(0, 2);
    };

    const revealedInfo = useMemo(() => {
        const info = {
            showDate: false,
            showTopic: false,
            showContext: false,
            showMembersSection: false,
            showVotingSection: false,
            currentPhase: '' as string,
            members: [] as RevealedMember[],
            typingSlots: [] as number[],
        };

        for (const msg of visibleMessages) {
            if (msg.id === 'welcome') {
                info.showDate = true;
                info.currentPhase = 'Welcome';
            }
            if (msg.id === 'topic') {
                info.showTopic = true;
            }
            if (msg.id === 'context') {
                info.showContext = true;
            }
            if (msg.id === 'intro-participants') {
                info.showMembersSection = true;
                info.currentPhase = 'Introductions';
                if (debate?.state?.members) {
                    for (const member of debate.state.members) {
                        if (!info.members.find(m => m.slot === member.slot)) {
                            info.members.push({
                                slot: member.slot,
                                name: member.name,
                                role: member.role,
                                hasIntroduced: false,
                                points: 0,
                            });
                        }
                    }
                }
            }
            if (msg.id.startsWith('intro-') && msg.type === 'member' && msg.slot !== undefined) {
                const member = info.members.find(m => m.slot === msg.slot);
                if (member) {
                    member.hasIntroduced = true;
                }
            }
            if (msg.id === 'phase-initial') {
                info.currentPhase = 'Initial Answers';
            }
            if (msg.id.startsWith('phase-round-')) {
                const roundNum = msg.id.replace('phase-round-', '');
                info.currentPhase = `Round ${roundNum}`;
            }
            if (msg.id.startsWith('phase-amend-')) {
                const roundNum = msg.id.replace('phase-amend-', '');
                info.currentPhase = `Round ${roundNum} - Amendments`;
            }
            if (msg.id === 'phase-voting') {
                info.showVotingSection = true;
                info.currentPhase = 'Voting';
            }
            if (msg.id === 'results') {
                info.currentPhase = 'Results';
            }
            if (msg.id === 'closing') {
                info.currentPhase = 'Complete';
            }
            if (msg.type === 'vote' && msg.pointsChange) {
                for (const change of msg.pointsChange) {
                    const member = info.members.find(m => m.slot === change.slot);
                    if (member) {
                        member.points += change.delta;
                    }
                }
            }
        }

        info.members.sort((a, b) => {
            if (b.points !== a.points) {
              return b.points - a.points;
            }
            return a.slot - b.slot;
        });

        return info;
    }, [visibleMessages, debate?.state?.members]);

    const typingSlots = useMemo(() => {
        if (!isTyping || currentIndex >= messages.length) {
          return [];
        }

        const nextMessage = messages[currentIndex];
        if (!nextMessage) {
          return [];
        }

        if (nextMessage.type === 'system') {
          return [];
        }

        if (nextMessage.slot !== undefined) {
            return [nextMessage.slot];
        }

        return [];
    }, [isTyping, currentIndex, messages]);

    const togglePlayPause = () => {
        setIsPlaying(!isPlaying);
    };

    const changeSpeed = (newSpeed: number) => {
        setSpeed(newSpeed);
        setShowSpeedMenu(false);
    };

    if (loading) {
        return (
            <div className={styles.loadingWrapper}>
                <Loading size="lg" text="Loading debate replay..." />
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

    const speedLabel = speed === 1000 ? '2x' : speed === 1600 ? '1.5x' : speed === 2500 ? '1x' : speed === 4000 ? '0.5x' : '1x';

    const hasAnyInfo = revealedInfo.showDate || revealedInfo.showTopic || revealedInfo.showContext || revealedInfo.showMembersSection;

    return (
        <div className={styles.page}>
            <div className={styles.chatHeader}>
                <div className={styles.chatTitle}>Debate Replay</div>
            </div>

            <div className={styles.mainContent}>
                <div ref={containerRef} className={styles.chatContainer}>
                {visibleMessages.map((msg, index) => {
                    const getSide = (): boolean => {
                        if (index === 0) {
                          return true;
                        }

                        let currentSide = true;
                        let lastSpeaker: string | null = null;

                        for (let i = 0; i <= index; i++) {
                            const currMsg = visibleMessages[i];
                            const speaker = currMsg.type === 'system' ? 'system' : `member-${currMsg.slot}`;

                            if (lastSpeaker !== null && speaker !== lastSpeaker) {
                                currentSide = !currentSide;
                            }
                            lastSpeaker = speaker;
                        }

                        return currentSide;
                    };

                    const isSystem = msg.type === 'system';
                    const isLeft = getSide();

                    return (
                        <div key={msg.id} className={`${styles.messageRow} ${isLeft ? styles.left : styles.right}`}>
                            {isLeft && (
                                <div
                                    className={styles.avatar}
                                    style={{ backgroundColor: isSystem ? getSystemColor() : getMemberColor(msg.slot!) }}
                                >
                                    {getMemberInitials(msg.name)}
                                </div>
                            )}

                            <div className={styles.messageContent}>
                                <div className={styles.messageHeader}>
                                    <span className={styles.messageName}>{msg.name}</span>
                                    {msg.role && <span className={styles.messageRole}>{msg.role}</span>}
                                </div>
                                <div
                                    className={`${styles.messageBubble} ${isLeft ? styles.bubbleLeft : styles.bubbleRight}`}
                                    style={{
                                        backgroundColor: isSystem
                                            ? 'rgba(99, 102, 241, 0.1)'
                                            : `${getMemberColor(msg.slot!)}15`,
                                        borderColor: isSystem
                                            ? getSystemColor()
                                            : getMemberColor(msg.slot!)
                                    }}
                                >
                                    {msg.type === 'vote' ? (
                                        <div className={styles.voteContent}>
                                            {msg.content.split('\n').map((line, i) => (
                                                <div key={i} className={styles.voteRankingLine}>
                                                    {line}
                                                </div>
                                            ))}
                                        </div>
                                    ) : (
                                        msg.content
                                    )}
                                </div>
                            </div>

                            {!isLeft && (
                                <div
                                    className={styles.avatar}
                                    style={{ backgroundColor: isSystem ? getSystemColor() : getMemberColor(msg.slot!) }}
                                >
                                    {getMemberInitials(msg.name)}
                                </div>
                            )}
                        </div>
                    );
                })}

                {isTyping && currentIndex < messages.length && (
                    <div className={styles.typingIndicator}>
                        <div className={styles.typingDot}></div>
                        <div className={styles.typingDot}></div>
                        <div className={styles.typingDot}></div>
                    </div>
                )}

                <div ref={messagesEndRef} />
            </div>

                {hasAnyInfo && (
                    <div className={styles.infoSidebar}>
                        {revealedInfo.showDate && (
                            <div className={styles.infoSection}>
                                <div className={styles.infoHeader}>
                                    <Calendar size={14} />
                                    <span>Date</span>
                                </div>
                                <p className={styles.infoText}>{formatDate(debate.createdAt)}</p>
                            </div>
                        )}

                        {revealedInfo.showTopic && (
                            <div className={styles.infoSection}>
                                <div className={styles.infoHeader}>
                                    <MessageSquare size={14} />
                                    <span>Topic</span>
                                </div>
                                <p className={styles.infoText}>{debate.question}</p>
                            </div>
                        )}

                        {revealedInfo.showContext && debate.context && (
                            <div className={styles.infoSection}>
                                <div className={styles.infoHeader}>
                                    <FileText size={14} />
                                    <span>Context</span>
                                </div>
                                <p
                                    className={`${styles.infoText} ${styles.contextText} ${isContextExpanded ? styles.contextExpanded : ''}`}
                                    onClick={() => setIsContextExpanded(!isContextExpanded)}
                                >
                                    {debate.context}
                                </p>
                            </div>
                        )}

                        {revealedInfo.currentPhase && (
                            <div className={styles.infoSection}>
                                <div className={styles.infoHeader}>
                                    <Activity size={14} />
                                    <span>Current Phase</span>
                                </div>
                                <div className={styles.currentPhase}>
                                    {revealedInfo.currentPhase}
                                </div>
                            </div>
                        )}

                        {revealedInfo.showMembersSection && (
                            <div className={styles.infoSection}>
                                <div className={styles.infoHeader}>
                                    {revealedInfo.showVotingSection ? <Trophy size={14} /> : <Users size={14} />}
                                    <span>{revealedInfo.showVotingSection ? 'Standings' : 'Participants'} ({revealedInfo.members.length})</span>
                                </div>
                                <div className={styles.membersList}>
                                    {revealedInfo.members.map((member, idx) => {
                                        const isTypingNow = typingSlots.includes(member.slot);
                                        const showRank = revealedInfo.showVotingSection && member.points > 0;
                                        return (
                                            <div
                                                key={member.slot}
                                                className={`${styles.memberItem} ${!member.hasIntroduced ? styles.memberNotIntroduced : ''}`}
                                            >
                                                {showRank && (
                                                    <div className={styles.memberRank}>
                                                        {idx === 0 ? '🥇' : idx === 1 ? '🥈' : idx === 2 ? '🥉' : `#${idx + 1}`}
                                                    </div>
                                                )}
                                                <div
                                                    className={styles.memberAvatar}
                                                    style={{ backgroundColor: getMemberColor(member.slot) }}
                                                >
                                                    {getMemberInitials(member.name)}
                                                </div>
                                                <div className={styles.memberDetails}>
                                                    <span className={styles.memberName}>{member.name}</span>
                                                    <span className={styles.memberRole}>{member.role}</span>
                                                </div>
                                                {revealedInfo.showVotingSection && member.points > 0 && (
                                                    <div className={styles.memberPoints}>
                                                        {member.points} pts
                                                    </div>
                                                )}
                                                {isTypingNow && (
                                                    <div className={styles.memberTyping}>
                                                        <span></span>
                                                        <span></span>
                                                        <span></span>
                                                    </div>
                                                )}
                                            </div>
                                        );
                                    })}
                                </div>
                            </div>
                        )}
                    </div>
                )}
            </div>

            <div className={styles.floatingControls}>
                <Button
                    variant="ghost"
                    size="sm"
                    icon={isPlaying ? <Pause size={18} /> : <Play size={18} />}
                    onClick={togglePlayPause}
                    className={styles.controlButton}
                >
                    {isPlaying ? 'Pause' : 'Play'}
                </Button>

                <div className={styles.speedControl}>
                    <Button
                        variant="ghost"
                        size="sm"
                        icon={<Gauge size={18} />}
                        onClick={() => setShowSpeedMenu(!showSpeedMenu)}
                        className={styles.controlButton}
                    >
                        {speedLabel}
                    </Button>

                    {showSpeedMenu && (
                        <div className={styles.speedMenu}>
                            <button onClick={() => changeSpeed(1000)} className={speed === 1000 ? styles.active : ''}>
                                2x Fast
                            </button>
                            <button onClick={() => changeSpeed(1600)} className={speed === 1600 ? styles.active : ''}>
                                1.5x
                            </button>
                            <button onClick={() => changeSpeed(2500)} className={speed === 2500 ? styles.active : ''}>
                                1x Normal
                            </button>
                            <button onClick={() => changeSpeed(4000)} className={speed === 4000 ? styles.active : ''}>
                                0.5x Slow
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
