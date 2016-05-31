let actions = {
  addComment: function(record) {
    return {
      type: 'ADD_COMMENT',
      record: record
    }
  },
  addComments: function(records) {
    return {
      type: 'ADD_COMMENTS',
      records: records
    }
  } 
}

export default actions
