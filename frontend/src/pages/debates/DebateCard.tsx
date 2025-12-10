import { useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Users, MessageCircle, Clock, Trophy, Play, Trash2, Edit, Film } from 'lucide-react';
import anime from 'animejs';
import type { Debate, DebatePhase } from '../../types';
import { Card, Button } from '../../components/ui';
import styles from './DebateCard.module.css';

interface DebateCardProps {
  debate: Debate;
  onRun?: (debate: Debate) => void;
  onDelete?: (debate: Debate) => void;
  onEdit?: (debate: Debate) => void;
  index?: number;
}

export function DebateCard({ debate, onRun, onDelete, onEdit, index = 0 }: DebateCardProps) {
  const cardRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (cardRef.current) {
      anime({
        targets: cardRef.current,
        opacity: [0, 1],
        translateY: [30, 0],
        scale: [0.95, 1],
        duration: 500,
        delay: index * 60,
        easing: 'easeOutCubic',
      });
    }
  }, [index]);

  const getPhaseLabel = (phase: DebatePhase): string => {
    switch (phase.toLowerCase()) {
      case 'created':
        return 'Created';
      case 'initialanswer':
        return 'Initial Answers';
      case 'crossquestion':
        return 'Cross Questions';
      case 'response':
        return 'Responses';
      case 'amendment':
        return 'Amendments';
      case 'finalanswer':
        return 'Final Answers';
      case 'ballot':
        return 'Voting';
      case 'completed':
        return 'Completed';
      case 'failed':
        return 'Failed';
      default:
        return phase;
    }
  };

  const getStatusBadgeClass = (status: string): string => {
    switch (status.toLowerCase()) {
      case 'created':
        return styles.statusCreated;
      case 'completed':
        return styles.statusCompleted;
      case 'failed':
        return styles.statusFailed;
      default:
        return styles.statusActive;
    }
  };

  const getCardStatusClass = (status: string): string => {
    switch (status.toLowerCase()) {
      case 'created':
        return styles.cardCreated;
      case 'completed':
        return styles.cardCompleted;
      case 'failed':
        return styles.cardFailed;
      default:
        return styles.cardActive;
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const winner = debate.state?.winner;

  return (
    <Card
      ref={cardRef}
      className={`${styles.card} ${getCardStatusClass(debate.status)}`}
      style={{ opacity: 0 }}
      variant="interactive"
      onClick={() => navigate(`/debates/${debate.id}`)}
    >
      <div className={styles.header}>
        <h3 className={styles.question}>{debate.question}</h3>
        <span className={`${styles.statusBadge} ${getStatusBadgeClass(debate.status)}`}>
          {getPhaseLabel(debate.status)}
        </span>
      </div>

      <div className={styles.meta}>
        <div className={styles.metaItem}>
          <Users size={14} />
          <span>{debate.nMembers} members</span>
        </div>
        <div className={styles.metaItem}>
          <MessageCircle size={14} />
          <span>{debate.nRounds} rounds</span>
        </div>
      </div>

      {winner ? (
        <div className={styles.winner}>
          <Trophy size={14} className={styles.winnerIcon} />
          <div className={styles.winnerInfo}>
            <span className={styles.winnerName}>{winner.member.name}</span>
          </div>
          <span className={styles.winnerPoints}>{winner.totalPoints} pts</span>
        </div>
      ) : debate.status.toLowerCase() === 'created' && (
        <div className={styles.readyIndicator}>
          <span>Ready to start</span>
        </div>
      )}

      <div className={styles.footer}>
        <div className={styles.debateDate}>
          <Clock size={12} />
          <span>{formatDate(debate.createdAt)}</span>
        </div>
        <div className={styles.actions} onClick={(e) => e.stopPropagation()}>
          {debate.status.toLowerCase() === 'created' && (
            <>
              {onEdit && (
                <Button
                  variant="ghost"
                  size="sm"
                  icon={<Edit size={14} />}
                  onClick={() => onEdit(debate)}
                  aria-label="Edit debate"
                />
              )}
              {onRun && (
                <Button
                  variant="ghost"
                  size="sm"
                  icon={<Play size={14} />}
                  onClick={() => onRun(debate)}
                  aria-label="Run debate"
                />
              )}
            </>
          )}
          {debate.status.toLowerCase() === 'completed' && (
            <Button
              variant="ghost"
              size="sm"
              icon={<Film size={14} />}
              onClick={() => navigate(`/debates/${debate.id}/replay`)}
              aria-label="Replay debate"
            />
          )}
          {onDelete && (
            <Button
              variant="ghost"
              size="sm"
              icon={<Trash2 size={14} />}
              onClick={() => onDelete(debate)}
              aria-label="Delete debate"
            />
          )}
        </div>
      </div>
    </Card>
  );
}
