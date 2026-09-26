import { actions } from '../bridge/actions';
import type { DialogSnapshot } from '../bridge/types';
import { cn } from '../lib/cn';

/**
 * Bottom-centre dialogue box. Mirrors the original `Dialogs/Talk` canvas: a name tag, the
 * streamed line and a row of choice buttons (`Continue` when the speaker has no options).
 */
export function DialogBox({ dialog }: { dialog: DialogSnapshot | undefined }) {
  const choices = dialog && dialog.choices && dialog.choices.length > 0 ? dialog.choices : ['Continue'];

  return (
    <view className={cn('dialog', dialog?.visible && 'dialog--open')}>
      {/* Choice buttons double as the white backdrop; the content box is layered on top. */}
      <view className="dialog__choices">
        {choices.map((choice, index) => (
          <button key={index} className="dialog__choice" onClick={() => actions.dialogSelect(index)}>
            <text className="dialog__choice-text">{choice}</text>
          </button>
        ))}
      </view>

      <view className="dialog__name">
        <text className="dialog__name-text">{dialog?.name ?? ''}</text>
      </view>

      <view className="dialog__content">
        <view className="dialog__inside">
          <text className="dialog__text">{dialog?.text ?? ''}</text>
        </view>
      </view>
    </view>
  );
}
