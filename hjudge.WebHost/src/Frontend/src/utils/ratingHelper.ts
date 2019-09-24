export function getRating(upvote: number, downvote: number) {
    let rating =
      upvote + downvote === 0 ?
        2.5 : Math.round(upvote * 500 / (upvote + downvote)) / 100.0;
    return rating;
}