import * as React from 'react';
import { setTitle } from '../../utils/titleHelper';
import { Get } from '../../utils/requestHelper';
import { tryJson } from '../../utils/responseHelper';
import { Placeholder } from 'semantic-ui-react';

interface RankProps {
  contestId: number,
  groupId: number
}

const Rank = (props: RankProps) => {
  const [loaded, setLoaded] = React.useState<boolean>(false);

  React.useEffect(() => {
    setTitle('排名');
    Get(`/rank/contest?contestId=${props.contestId ? props.contestId : 0}&groupId=${props.groupId ? props.groupId : 0}`)
      .then(tryJson)
      .then(data => {
        setLoaded(true);
        console.log(data);
      })
  }, []);

  const placeHolder = <Placeholder>
    <Placeholder.Paragraph>
      <Placeholder.Line />
      <Placeholder.Line />
      <Placeholder.Line />
      <Placeholder.Line />
    </Placeholder.Paragraph>
  </Placeholder>;
  return loaded ? <></> : placeHolder;
}

export default Rank;