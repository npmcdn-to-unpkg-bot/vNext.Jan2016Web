function getId(state) {
  return state.comments.reduce((maxId, comment) => {
    return Math.max(comment.id, maxId)
  }, -1) + 1
}

let reducer = function(state, action) {
  switch (action.type) {
    case 'ADD_COMMENT':
      let record = action.record;
      record.id = getId(state);
      return Object.assign({}, state, {
        comments: [record, ...state.comments]
      })
    case 'ADD_COMMENTS':
      let records = action.records;
     
      return Object.assign({}, state, {
        comments: records.concat(state.comments)
      })
    default: 
      return state;
  }
}

export default reducer
