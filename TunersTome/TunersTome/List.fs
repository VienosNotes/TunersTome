namespace TunersTume
module ListEx =
    let update (pred:('a -> bool)) (action:('a -> 'a)) (list:'a list) =
        let updateInner pred item =
            if pred item then
                action item
            else
                item                                    
        List.map (updateInner pred) list         
