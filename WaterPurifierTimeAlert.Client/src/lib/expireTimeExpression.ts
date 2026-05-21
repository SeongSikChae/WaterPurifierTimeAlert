// 서버 측 ANTLR Expression 문법에 대응하는 클라이언트 사전 유효성 검사.
//
// grammar Expression;
// expression   : nowPart? modifierPart? snapPart?;
// nowPart      : 'now';
// modifierPart : operator factor? timeUnit;
// operator     : '+' | '-';
// factor       : [0-9]+;
// timeUnit     : 'mon' | 's' | 'm' | 'h' | 'd' | 'w' | 'q' | 'y';
// snapPart     : '@' snapTimeUnit modifierPart?;
// snapTimeUnit : 'mon' | 's' | 'm' | 'h' | 'd' | 'w' | 'w0'..'w7' | 'q' | 'y';
// WS           : [ \t\n\r]+ -> skip;
//
// 주의: 'mon'은 'm'보다 먼저 매칭되어야 한다.

const TIME_UNIT = '(?:mon|s|m|h|d|w|q|y)';
const SNAP_TIME_UNIT = '(?:mon|s|m|h|d|w[0-7]?|q|y)';
const MODIFIER = `(?:[+-]\\s*\\d*\\s*${TIME_UNIT})`;
const SNAP = `(?:@\\s*${SNAP_TIME_UNIT}(?:\\s*${MODIFIER})?)`;
const NOW = '(?:now)';

const EXPRESSION = new RegExp(
  `^\\s*(?:${NOW})?\\s*(?:${MODIFIER})?\\s*(?:${SNAP})?\\s*$`,
);

export function isValidExpireTimeExpression(expression: string): boolean {
  if (!expression) return false;
  const trimmed = expression.trim();
  if (trimmed.length === 0) return false;
  return EXPRESSION.test(expression);
}

export const EXPIRE_TIME_HELP =
  '예: "now+6mon", "+1y", "-30d", "@mon", "now@w0+1d". ' +
  '단위: y(년) q(분기) mon(월) w(주, snap에서 w0~w7 가능) d(일) h(시) m(분) s(초)';
